using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Htmlilka;
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

        public Image(string filePath) : base() {
            SourcePath = ConvertPathToAbsolute(filePath);
            if (!File.Exists(SourcePath)) {
                Log.Error.OnObject(this, $"File {filePath} not found");
            }
        }

        public Uri GetUri() {
            if (ImageCache == null) {
                GenerateResources();
            }

            return ImageCache.GetFileUri(ImageCache.Mips.First().Value);
        }

        public float[] GetRoi() { 
            if (ImageCache == null) {
                GenerateResources();
            }

            return ImageCache.Roi;
        }

        public bool IsRoiFitsIntoWideRect(float[] roi) {
            if (roi != null && roi.Length > 0) {
                if (ImageCache == null) {
                    GenerateResources();
                }

                var imageWidth = (float)ImageCache.Mips.First().Key;
                var imageHeight = imageWidth * ImageCache.Aspect;

                var x0 = roi[0] / 100f;
                var x1 = roi[1] / 100f;
                var y0 = roi[2] / 100f;
                var y1 = roi[3] / 100f;

                var roiWidth = (x1 - x0) * imageWidth;
                var roiHeight = (y1 - y0) * imageHeight;

                var wideRectAspect = 9f / 16f;
                var wideRectHeight = roiHeight;
                var wideRectWidth = roiWidth;

                if (roiWidth > roiHeight) {
                    wideRectHeight = wideRectAspect * wideRectWidth;
                } else {
                    wideRectWidth = wideRectHeight / wideRectAspect;
                }

                return wideRectHeight <= imageHeight && wideRectWidth <= imageWidth && 
                        wideRectWidth >= roiWidth && wideRectHeight >= roiHeight;
            }

            return false;
        }

        private void GenerateResources() {
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

        public override Node Generate(Context context) {
            if (ImageCache == null) {
                GenerateResources();
            }

            var biggestMip = ImageCache.Mips.First();
            var uri = ImageCache.GetFileUri(biggestMip.Value);

            var result = new VoidTag("img")
                .Attribute("src", uri.ToString());

            if (ImageCache.Roi != null) {
                result = new Tag("div")
                    .Attribute("style", "overflow: hidden;")
                    .Add(result)
                    .Add(new Behaviour("RoiImage", ImageCache.Aspect, ImageCache.Roi).Generate(context));
            } else if (!ImageCache.IsVectorImage) {
                result.Attribute("style", $"max-width: {biggestMip.Key}px;");
            }

            result.AddClasses("Image");

            return result;
        }
    }


}