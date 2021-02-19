using Htmlilka;
using System.Collections.Generic;

namespace Csml {
    /*public class TemplateLeftSideMenuBehaviour : Behaviour {
        public TemplateLeftSideMenuBehaviour(int contentWidth, int anchorLineWidth) : base("TemplateLeftSideMenu", contentWidth, anchorLineWidth) { }
    };*/

    public abstract class TemplateLeftSideMenu<T> : Template<T> where T: TemplateLeftSideMenu<T> {
        private readonly IEnumerable<IElement> LeftSideElements;
        private readonly IEnumerable<IElement> RightSideElements;
        private readonly int LeftWidth;
        private readonly int RightWidth;
        private readonly int ContentWidth;
        private readonly int AnchorLineWidth;
        private readonly IElement HeaderLogo;
        public TemplateLeftSideMenu(IElement headerLogo, IEnumerable<IElement> leftSideElements, int leftWidth, IEnumerable<IElement> rightSideElems, int rightWidth, int contentWidth, int anchorLineWidth) {
            this.HeaderLogo = headerLogo;
            this.LeftSideElements = leftSideElements;
            this.RightSideElements = rightSideElems;
            this.LeftWidth = leftWidth;
            this.RightWidth = rightWidth;
            this.ContentWidth = contentWidth;
            this.AnchorLineWidth = anchorLineWidth;
        }

        public abstract Tag WriteMaterial(Context context, IMaterial material);

        public override void ModifyBody(Tag body, Context context, IMaterial material) {
            base.ModifyBody(body, context, material);

            body.Add((HeaderLogo.Generate(context) as Tag).AddClasses("Logo"));



            body.Add(new Behaviour("TemplateLeftSideMenuInit", LeftWidth, RightWidth, ContentWidth, AnchorLineWidth).Generate(context));

            AddContainer(body, context, "LeftSideContainer", LeftSideElements);
            AddContainer(body, context, "RightSideContainer", RightSideElements);

            context.EstimatedWidth = ContentWidth;
            body.Add(WriteMaterial(context, material)
                .AddClasses("Content anchorsOutside")
                );

            body.Add(new Behaviour("TemplateLeftSideMenuAlign").Generate(context));
        }

        private void AddContainer(Tag body, Context context, string className, IEnumerable<IElement> elements){
            if(elements == null)
                return;

            var container = new Tag("div").AddClasses(className);

            bool isEmpty = true;
            foreach (var e in elements) {
                container.Add(e.Generate(context));
                isEmpty = false;
            }

            if(isEmpty)
                return;

            body.Add(container);
        }
    }

}