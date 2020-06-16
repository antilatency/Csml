using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public sealed class TemplateRegularMaterial : TemplateRegularMaterial<TemplateRegularMaterial> {
        public TemplateRegularMaterial(IElement leftSideMenu) : base(leftSideMenu) { }
    }

    public class TemplateRegularMaterial<T> : TemplateLeftSideMenu<T> where T: TemplateRegularMaterial<T> {
        public TemplateRegularMaterial(IElement leftSideMenu) : base(leftSideMenu,800,64) { }

        public override HtmlNode WriteMaterial(Context context, IMaterial material) {
            return HtmlNode.CreateNode("<div>").Do(x => {
                x.Add("<div>", "Header").Do(x => {
                    x.Add($"<h1>", "Title").AddTextWithWordBreaks(material.Title);
                    if (material.TitleImage != null) {
                        x.Add(material.TitleImage.Generate(context));
                    }
                    x.Add(material.Description.Generate(context));
                });
                x.Add(material.Content.Generate(context));
            });
        }
    }
}