using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Csml {
    public class MaterialCard: Element<MaterialCard> {
        Func<Context,IMaterial> MaterialGetter;

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
            MaterialGetter = (context)=>material;
        }
        public MaterialCard(ILanguageSelector<IMaterial> languageSelector) {
            MaterialGetter = (context) => languageSelector[context.Language];
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var material = MaterialGetter(context);
            yield return HtmlNode.CreateNode(context.AForbidden?"<div>":"<a>").Do(x => {                
                if (!context.AForbidden)
                    x.SetAttributeValue("href", material.GetUri(context));
                x.AddClass("MaterialCard");

                context.AForbidden = true;
                
                if (material.TitleImage != null) {
                    x.Add(material.TitleImage.Generate(context));
                } else {
                    Log.Warning.OnObject(this, "TitleImage of material required");
                    x.Add(CsmlPredefined.MissingImage.Generate(context));
                }

                
                x.Add("<div>").Do(x=> {
                    x.AddClass("title");
                    x.InnerHtml = material.Title;
                });                
                x.Add(material.Description.Generate(context));
            });

        }
    }
}