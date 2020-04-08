using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HtmlAgilityPack;
using ImageMagick;

namespace Csml {

    public partial class ImageInfo : Scope<ImageInfo> { 
    
    }


    public sealed class Image : Image<Image> {
        public Image(string filePath) : base(filePath) {
            
        }
    }
    
    
    public class Image<T> : Element<T> where T : Image<T> {
        public static readonly int MinImageWidth = 128;
        public string SourcePath { get; private set; }        
        
        private bool IsResourcesGenerated = false;
        private string Hash;
        private List<KeyValuePair<int, string>> Mips;
        private float Aspect = 1;
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
            var directory = Path.Combine(context.OutputRootDirectory, context.GetSubDirectoryFromSourceAbsoluteFilePath(SourcePath));
            var extension = Path.GetExtension(SourcePath);
            Utils.CreateDirectory(directory);

            Hash = Utils.ToHashString(File.ReadAllBytes(SourcePath));

            Mips = new List<KeyValuePair<int, string>>();

            var i = new MagickImage(SourcePath);
            Aspect = i.Height / (float)i.Width;

            Func<int, string> outputPath = x=>Path.Combine(directory, Hash+x+extension);
            var w = i.Width;

            if (!File.Exists(outputPath(w)) | context.ForceRebuildImages)
                File.Copy(SourcePath, outputPath(w));            
            Mips.Add(new KeyValuePair<int, string>(w, outputPath(w)));

            while (MinImageWidth <= w / 2) {
                if (!File.Exists(outputPath(w / 2)) | context.ForceRebuildImages) {
                    i.Resize(i.Width / 2, i.Height / 2);
                    
                    i.Write(outputPath(i.Width));
                }
                w /= 2;
                Mips.Add(new KeyValuePair<int, string>(w, outputPath(w)));
            }         
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (!IsResourcesGenerated) {
                GenerateResources(context);
                IsResourcesGenerated = true;
            }

            var result = HtmlNode.CreateNode("<img></img>");
            result.SetAttributeValue("src", Mips[0].Value);
            
            
            foreach (var e in base.Generate(context)) {
                result.AppendChild(e);
            }


            var roiFilePath = Path.ChangeExtension(SourcePath, ".roi");
            if (File.Exists(roiFilePath)) {
                var script = context.Head.ChildNodes.Where(x => x.Id == "resizeRoiImages").FirstOrDefault();
                if (script == null) {
                    var code = File.ReadAllText(Path.Combine(Path.ChangeExtension(Utils.ThisFilePath(), null), "resizeRoiImages.html"));
                    context.Head.Add(code);
                }
                var roi = File.ReadAllText(roiFilePath);

                result = result.Wrap("<div>");

                result.SetAttributeValue("style", "overflow: hidden;");
                result.SetAttributeValue("data-roi", roi);
                result.SetAttributeValue("data-aspect", Aspect.ToString());
                result.SetAttributeValue("class", "roi-image-container");
            }

            result.AddClass("image");



            yield return result;
        }
    }


}