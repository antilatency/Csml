using ImageMagick;
using System;

namespace Csml {
    public interface IImage : IElement {
        public ImageCache GetImageCache();

        public float[] GetRoi();

        public bool IsRoiFitsIntoWideRect(float[] roi);

        public MagickColor GetTopLeftPixel();

        public Uri GetImageUri();

        public Image GetImage();
    }
}
