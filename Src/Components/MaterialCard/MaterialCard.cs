using Htmlilka;
using System;

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

        public override Node Generate(Context context) {
            var material = MaterialGetter(context);
            Tag result;
            if(!context.AForbidden) {
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
            var riseUpContent = new Tag("div") {
                new Title(material.Title, " .").Generate(context),
                material.Description.Generate(context)
            };
            riseUpContent.AddClasses("RiseUpContent");
            result.Add(riseUpContent);
            //result.AddDiv(x => {
            //    //x.AddClasses("Title");
            //    // x.Add(new Title(material.Title, " .").Generate(context));
            //    //x.AddPureHtmlNode(material.Title.Replace(".", "<wbr/>."));
            //    //x.AddText(material.Title);
            //});
            result.Add(new Behaviour("MaterialCard").Generate(context));


            return result;
        }
    }
}