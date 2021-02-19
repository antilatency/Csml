using System;
using System.Drawing;
using Htmlilka;

namespace Csml {
    public class MaterialCard : Element<MaterialCard> {
        Func<Context, IMaterial> MaterialGetter;

        /*public MaterialCard(IElement element) {
            if (element is IMaterial) {
                MaterialGetter = (context) => (element as IMaterial);
                return;
            }
            if (element is ILanguageSelector<IMaterial>) {
                MaterialGetter = (context) => (element as ILanguageSelector<IMaterial>)[context.Language];
                return;
            }
            Log.Error.OnCaller("Only IMaterial or LanguageSelector<IMaterial> expected.");
        }*/
        public MaterialCard(IMaterial material) {
            MaterialGetter = (context) => material;
        }
        public MaterialCard(ILanguageSelector<IMaterial> languageSelector) {
            MaterialGetter = (context) => languageSelector[context.Language];
        }

        private string PickBackgroundColor(string imagePath) {
            var bitmap = new Bitmap(imagePath);
            var color = bitmap.GetPixel(0, 0);
            var result = color.ToString();
            return "null";
        }

        private string textColor(IImage image) {
            var pixel = image.GetTopLeftPixel();
            return (pixel.R + pixel.G + pixel.B) / 3 > 255 / 2 ? "black" : "white";
        }
        public override Node Generate(Context context) {
            var material = MaterialGetter(context);
            Tag result;
            if (!context.AForbidden) {
                result = new Tag("a");
                result.Attribute("href", material.GetUri(context.Language));
            } else {
                result = new Tag("div");
            }
            context.AForbidden = true;
            result.AddClasses("MaterialCard");
            if(material.TitleImage != null) {
                result.Add(material.TitleImage.Generate(context));
            } else {
                Log.Warning.OnObject(this, "TitleImage of material required");
                result.Add(CsmlPredefined.MissingImage.Generate(context));
            }
            result.Add(getSlideContent(material, context));
            result.Add(new Behaviour("MaterialCard").Generate(context));

            return result;
        }

        private Tag getSlideContent(IMaterial material, Context context) {
            Tag result = new Tag("div");
            result.AddClasses("MaterialCardSlider");
            result.Add(new Title(material.Title, " .", 4).Generate(context));
            result.Add(material.Description.Generate(context));
            return result;
        }
    }
}