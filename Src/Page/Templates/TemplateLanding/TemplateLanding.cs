using Htmlilka;
using System.Collections.Generic;

namespace Csml {
    public sealed class TemplateLanding : TemplateLanding<TemplateLanding> {
        public TemplateLanding(IElement headerLogo, IEnumerable<IElement> leftSideElements, IEnumerable<IElement> rightSideElements) : base(headerLogo, leftSideElements, rightSideElements) { }
    }

    public class TemplateLanding<T> : TemplateLeftSideMenu<T> where T : TemplateLanding<T> {
        public TemplateLanding(IElement headerLogo, IEnumerable<IElement> leftSideElements, IEnumerable<IElement> rightSideElements) : base(headerLogo, leftSideElements, 260, rightSideElements, 60, 1280, 64) { }

        public override Tag WriteMaterial(Context context, IMaterial material) {
            return new Tag("div")
                .AddDiv(a => {
                    a.AddClasses("Header");
                    if(material.TitleImage != null) {
                        a.Add(material.TitleImage.Generate(context));
                    }
                    a.Add(material.Description.Generate(context));
                })
                .Add(material.Content.Generate(context));

        }
    }
}