using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HtmlAgilityPack;
using ImageMagick;
using Newtonsoft.Json;

namespace Csml {

    public class ImageCache : Cache<ImageCache> {
        public float Aspect = 1;
        public float[] Roi;
        public Dictionary<int, string> Mips;
    }

    public sealed class Image : Image<Image> {
        public Image(string filePath) : base(filePath) {
            
        }
    }
    
    
    public class Image<T> : Element<T> where T : Image<T> {
        
        public static readonly int MinImageWidth = 128;
        public string SourcePath { get; private set; }        
        
        //private bool IsResourcesGenerated = false;
        //private string OutputSubDirectory;

        protected ImageCache ImageCache;



        public Image(string filePath):base() {
            SourcePath = ConvertPathToAbsolute(filePath);
        }

        /*private ImageCodecInfo GetEncoder(ImageFormat format) {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
        }*/

        private void GenerateResources(Context context) {
            //OutputSubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(SourcePath);

            //var outputDirectory = Path.Combine(context.OutputRootDirectory, OutputSubDirectory);
            var extension = Path.GetExtension(SourcePath);
            //Utils.CreateDirectory(outputDirectory);

            //TODO: file not found
            var hash = Utils.ToHashString(File.ReadAllBytes(SourcePath));

            ImageCache = ImageCache.Load(hash);

            Func<int, string> outputFileName = x => hash + x + extension;
            Func<int, string> outputPath = x => Path.Combine(ImageCache.Directory, outputFileName(x));

            if (ImageCache == null) {
                ImageCache = ImageCache.Create(hash);
                ImageCache.Mips = new Dictionary<int, string>();

                var i = new MagickImage(SourcePath);
                ImageCache.Aspect = i.Height / (float)i.Width;

                
                var w = i.Width;

                File.Copy(SourcePath, outputPath(w));
                ImageCache.Mips.Add(w, outputFileName(w));

                while (MinImageWidth <= w / 2) {
                    i.Resize(i.Width / 2, i.Height / 2);
                    i.Write(outputPath(i.Width));                    
                    w = i.Width;
                    ImageCache.Mips.Add(w, outputFileName(w));
                }
                

                var roiFilePath = Path.ChangeExtension(SourcePath, ".roi");
                if (File.Exists(roiFilePath)) {
                    //var File.ReadAllText(roiFilePath);
                    ImageCache.Roi = JsonConvert.DeserializeObject<float[]>(Utils.ReadAllText(roiFilePath));
                    
                }

                ImageCache.Save();
            }


            /*using (new Stopwatch("Parallel image copy")) {
                ImageCache.Mips.AsParallel().ForAll(x => {
                    var source = Path.Combine(ImageCache.Directory, x.Value);
                    var dest = Path.Combine(outputDirectory, x.Value);
                    File.Copy(source, dest);
                });
            }*/
            //using (new Stopwatch("Sequental image copy")) {
                /*foreach (var f in ImageCache.Mips) {
                    var source = Path.Combine(ImageCache.Directory, f.Value);
                    var dest = Path.Combine(outputDirectory, f.Value);
                    File.Copy(source, dest);
                }*/
            //}

        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (ImageCache == null) {
                GenerateResources(context);
            }

            var biggestMip = ImageCache.Mips.First();


            Uri uri = ImageCache.GetFileUri(biggestMip.Value);// new Uri(context.BaseUri, Path.Combine(OutputSubDirectory, biggestMip.Value));


            var result = HtmlNode.CreateNode("<img>").Do(x => {
                x.SetAttributeValue("src", uri.ToString());
                x.Add(base.Generate(context));
            });

           


            if (ImageCache.Roi != null) {
                result = result.Wrap("<div>");
                result.Add(new Behaviour("RoiImage", ImageCache.Aspect, ImageCache.Roi).Generate(context));
                result.SetAttributeValue("style", "overflow: hidden;");


                /*var script = context.Head.ChildNodes.Where(x => x.Id == "resizeRoiImages").FirstOrDefault();
                if (script == null) {
                    var code = File.ReadAllText(Path.Combine(Path.ChangeExtension(Utils.ThisFilePath(), null), "resizeRoiImages.html"));
                    context.Head.Add(code);
                }*/


                /*result.SetAttributeValue("style", "overflow: hidden;");
                result.SetAttributeValue("data-roi", ImageCache.Roi);
                result.SetAttributeValue("data-aspect", ImageCache.Aspect.ToString());
                result.SetAttributeValue("class", "roi-image-container");*/
            } else {
                result.SetAttributeValue("style", $"max-width: {biggestMip.Key}px;");
            }

            result.AddClass("image");
            yield return result;
        }
    }


}