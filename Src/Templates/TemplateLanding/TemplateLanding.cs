using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public sealed class TemplateLanding : TemplateLanding<TemplateLanding> {
        public TemplateLanding(IElement leftSideMenu) : base(leftSideMenu) { }
    }

    public class TemplateLanding<T> : TemplateLeftSideMenu<T> where T : TemplateLanding<T> {
        public TemplateLanding(IElement leftSideMenu) : base(leftSideMenu, 1280, 0) { }

        public override HtmlNode WriteMaterial(Context context, IMaterial material) {
            return HtmlNode.CreateNode("<div>").Do(x => {
                x.Add(material.Content.Generate(context));
            });
        }
    }
}