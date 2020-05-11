using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace Csml {
    public class MaterialCard: Element<MaterialCard> {
        Func<Context,IMaterial> MaterialGetter;

        public MaterialCard(IElement element) {
            if (element is IMaterial) {
                MaterialGetter = (context) => (element as IMaterial);
                return;
            }
            if (element is LanguageSelector<IMaterial>) {
                MaterialGetter = (context) => (element as LanguageSelector<IMaterial>)[context.Language];
                return;
            }
            Log.Error.OnCaller("Only IMaterial or LanguageSelector<IMaterial> expected.");
        }
        public MaterialCard(IMaterial material) {
            MaterialGetter = (context)=>material;
        }
        public MaterialCard(LanguageSelector<IMaterial> languageSelector) {
            MaterialGetter = (context) => languageSelector[context.Language];
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var material = MaterialGetter(context);
            yield return HtmlNode.CreateNode("<a>").Do(x => {
                x.SetAttributeValue("href", material.GetUri(context));
                x.AddClass("card");
                x.Add(material.TitleImage.Generate(context));
                x.Add("<div>").Do(x=> {
                    x.AddClass("title");
                    x.InnerHtml = material.Title;
                });                
                x.Add(material.Description.Generate(context));
            });

        }
    }
}