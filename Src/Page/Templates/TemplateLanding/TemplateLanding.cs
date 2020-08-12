using Htmlilka;

namespace Csml {
    public sealed class TemplateLanding : TemplateLanding<TemplateLanding> {
        public TemplateLanding(IElement leftSideMenu) : base(leftSideMenu) { }
    }

    public class TemplateLanding<T> : TemplateLeftSideMenu<T> where T : TemplateLanding<T> {
        public TemplateLanding(IElement leftSideMenu) : base(leftSideMenu, 1280, 0) { }

        public override Tag WriteMaterial(Context context, IMaterial material) {
            return new Tag("div")
                .Add(material.Content.Generate(context));
            
        }
    }
}