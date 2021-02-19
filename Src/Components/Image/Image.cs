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
        public MagickColor TopLeftPixelColor;
    }

    public class Image : Element<Image>, IImage {

        public static readonly int MinImageWidth = 128;
        public string SourcePath { get; private set; }

        protected ImageCache ImageCache;

        public Image(string filePath) : base() {
            SourcePath = ConvertPathToAbsolute(filePath);
            if(!File.Exists(SourcePath)) {
                Log.Error.OnObject(this, $"File {filePath} not found");
            }
        }

        public ImageCache GetImageCache() {
            if(ImageCache == null) {
                GenerateResources(SourcePath, ref ImageCache);;
            }

            return ImageCache;
        }

        public Uri GetImageUri() {
            if(ImageCache == null) {
                GenerateResources(SourcePath, ref ImageCache);
            }

            return ImageCache.GetFileUri(ImageCache.Mips.First().Value);
        }

        public float[] GetRoi() {
            if(ImageCache == null) {
                GenerateResources(SourcePath, ref ImageCache);
            }

            return ImageCache.Roi;
        }

        public MagickColor GetTopLeftPixel() {
            if(ImageCache == null) {
                GenerateResources(SourcePath, ref ImageCache);
            }

            return ImageCache.TopLeftPixelColor;
        }

        public bool IsRoiFitsIntoWideRect(float[] roi) {
            if(roi != null && roi.Length > 0) {
                if(ImageCache == null) {
                    GenerateResources(SourcePath, ref ImageCache);
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

                if(roiWidth > roiHeight) {
                    wideRectHeight = wideRectAspect * wideRectWidth;
                } else {
                    wideRectWidth = wideRectHeight / wideRectAspect;
                }

                return wideRectHeight <= imageHeight && wideRectWidth <= imageWidth &&
                        wideRectWidth >= roiWidth && wideRectHeight >= roiHeight;
            }

            return false;
        }

        public static void GenerateResources(string sourcePath, ref ImageCache imageCache) {
            var extension = Path.GetExtension(sourcePath);
            var hash = Hash.CreateFromFile(sourcePath).ToString();
            imageCache = ImageCache.Load(hash);
            if(imageCache != null) { return; }
            imageCache = ImageCache.Create(hash);
            var direcotry = imageCache.Directory;
            string outputFileName(int x) => hash + x + extension;
            string outputPath(int x) => Path.Combine(direcotry, outputFileName(x));
            var image = new MagickImage(sourcePath);

            imageCache.TopLeftPixelColor = image.GetPixels()[0, 0].ToColor();
            imageCache.Aspect = image.Height / (float)image.Width;
            imageCache.Mips = new Dictionary<int, string>();
            imageCache.IsVectorImage = image.Format == MagickFormat.Svg;
            imageCache.IsAnimatedImage = image.Format == MagickFormat.Gif;

            var mipWidth = image.Width;
            if(File.Exists(outputPath(mipWidth))) { File.Delete(outputPath(mipWidth)); }
            File.Copy(sourcePath, outputPath(mipWidth));
            imageCache.Mips.Add(mipWidth, outputFileName(mipWidth));

            if(!(imageCache.IsVectorImage || imageCache.IsAnimatedImage)) {
                while(MinImageWidth <= mipWidth / 2) {
                    image.Resize(image.Width / 2, image.Height / 2);
                    image.Write(outputPath(image.Width));
                    PngUtils.NormalizeChunks(outputPath(image.Width)); //to prevent commit after every conversion;
                    mipWidth = image.Width;
                    imageCache.Mips.Add(mipWidth, outputFileName(mipWidth));
                }
            }

            var roiFilePath = Path.ChangeExtension(sourcePath, ".roi");

            if(File.Exists(roiFilePath)) {
                imageCache.Roi = JsonConvert.DeserializeObject<float[]>(Utils.ReadAllText(roiFilePath));
            }

            imageCache.Save();
        }

        private string BackgroundColor() {
            var pixel = GetTopLeftPixel();
            return $"rgba({pixel.R}, {pixel.G}, {pixel.B}, {pixel.A})";
        }

        public override Node Generate(Context context) {
            if(ImageCache == null) {
                GenerateResources(SourcePath, ref ImageCache);
            }

            var biggestMip = ImageCache.Mips.First();
            var uri = ImageCache.GetFileUri(biggestMip.Value);

            var result = new VoidTag("img")
                .Attribute("src", uri.ToString());

            if(ImageCache.Roi != null) {
                result = new Tag("div")
                    .Attribute("style", $"overflow: hidden; background-color: {BackgroundColor()};")
                    //.Attribute("style", $"background-color: {BackgroundColor()};")
                    .Add(result)
                    //.Attribute("style", $"background-color: {BackgroundColor()};")
                    .Add(new Behaviour("RoiImage", ImageCache.Aspect, ImageCache.Roi).Generate(context));
            } else if(!ImageCache.IsVectorImage) {
                result.Attribute("style", $"max-width: {biggestMip.Key}px;");
            }

            result.AddClasses("Image");

            return result;
        }

        public Image GetImage() {
            return this;
        }
    }


}