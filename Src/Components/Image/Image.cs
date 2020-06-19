using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using ImageMagick;
using Newtonsoft.Json;

namespace Csml {

    [CacheConfig("Images", true)]
    public class ImageCache : Cache<ImageCache> {
        public float Aspect = 1;
        public float[] Roi;
        public bool IsVectorImage;
        public bool IsAnimatedImage;
        public Dictionary<int, string> Mips;
    }
    
    public class Image : Element<Image> {
        
        public static readonly int MinImageWidth = 128;
        public string SourcePath { get; private set; }        

        protected ImageCache ImageCache;

        public Image(string filePath):base() {
            SourcePath = ConvertPathToAbsolute(filePath);
            if (!File.Exists(SourcePath)) {
                Log.Error.OnObject(this, $"File {filePath} not found");
            }
        }

        private void GenerateResources(Context context) {
            var extension = Path.GetExtension(SourcePath);
            var hash = Hash.CreateFromFile(SourcePath).ToString();

            ImageCache = ImageCache.Load(hash);

            Func<int, string> outputFileName = x => hash + x + extension;
            Func<int, string> outputPath = x => Path.Combine(ImageCache.Directory, outputFileName(x));

            if (ImageCache == null) {
                ImageCache = ImageCache.Create(hash);

                var image = new MagickImage(SourcePath);

                ImageCache.Aspect = image.Height / (float)image.Width;
                ImageCache.Mips = new Dictionary<int, string>();
                ImageCache.IsVectorImage = image.Format == MagickFormat.Svg;
                ImageCache.IsAnimatedImage = image.Format == MagickFormat.Gif;

                var mipWidth = image.Width;

                File.Copy(SourcePath, outputPath(mipWidth));
                ImageCache.Mips.Add(mipWidth, outputFileName(mipWidth));

                if (!(ImageCache.IsVectorImage || ImageCache.IsAnimatedImage)) { 
                    while (MinImageWidth <= mipWidth / 2) {
                        image.Resize(image.Width / 2, image.Height / 2);
                        image.Write(outputPath(image.Width));                    
                        mipWidth = image.Width;
                        ImageCache.Mips.Add(mipWidth, outputFileName(mipWidth));
                    }
                }

                var roiFilePath = Path.ChangeExtension(SourcePath, ".roi");

                if (File.Exists(roiFilePath)) {
                    ImageCache.Roi = JsonConvert.DeserializeObject<float[]>(Utils.ReadAllText(roiFilePath));
                }

                ImageCache.Save();
            }
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (ImageCache == null) {
                GenerateResources(context);
            }

            var biggestMip = ImageCache.Mips.First();
            var uri = ImageCache.GetFileUri(biggestMip.Value);


            var result = HtmlNode.CreateNode("<img>").Do(x => {
                x.SetAttributeValue("src", uri.ToString());
                x.Add(base.Generate(context));
            });


            if (ImageCache.Roi != null) {
                result = result.Wrap("<div>");
                result.Add(new Behaviour("RoiImage", ImageCache.Aspect, ImageCache.Roi).Generate(context));
                result.SetAttributeValue("style", "overflow: hidden;");
            } else if (!ImageCache.IsVectorImage) {
                result.SetAttributeValue("style", $"max-width: {biggestMip.Key}px;");
            }

            result.AddClass("Image");

            yield return result;
        }
    }


}