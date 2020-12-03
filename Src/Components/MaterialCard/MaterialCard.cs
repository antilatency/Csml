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

        public override Node Generate(Context context) {
            static string backgroundColor(Image image) {
                var pixel = image.GetTopLeftPixel();
                return $"rgba({pixel.R}, {pixel.G}, {pixel.B}, 0.5)";
            }
            static string textColor(Image image) {
                var pixel = image.GetTopLeftPixel();
                return (pixel.R + pixel.G + pixel.B) / 3 > 255 / 2 ? "black" : "white";
            }
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
            if (material.TitleImage != null) {
                result.Add(material.TitleImage.Generate(context));
                result.Attribute("style", $"background-color: {backgroundColor(material.TitleImage)}; " +
                    $"color:{textColor(material.TitleImage)};");
            } else {
                Log.Warning.OnObject(this, "TitleImage of material required");
                result.Add(CsmlPredefined.MissingImage.Generate(context));
                result.Attribute("style", $"background-color: {backgroundColor(CsmlPredefined.MissingImage)}; " +
                    $"color: {textColor(CsmlPredefined.MissingImage)};");
            }
            var riseUpContent = new Tag("div") {
                new Title(material.Title, " .").Generate(context),
                material.Description.Generate(context)
            };
            riseUpContent.AddClasses("RiseUpContent");
            result.Add(riseUpContent);
            result.Add(new Behaviour("MaterialCard").Generate(context));

            return result;
        }
    }
}