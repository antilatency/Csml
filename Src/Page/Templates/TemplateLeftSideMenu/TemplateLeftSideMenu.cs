using Htmlilka;

namespace Csml {
    /*public class TemplateLeftSideMenuBehaviour : Behaviour {
        public TemplateLeftSideMenuBehaviour(int contentWidth, int anchorLineWidth) : base("TemplateLeftSideMenu", contentWidth, anchorLineWidth) { }
    };*/

    public abstract class TemplateLeftSideMenu<T> : Template<T> where T: TemplateLeftSideMenu<T> {
        private IElement LeftSideMenu;
        private int ContentWidth;
        private int AnchorLineWidth;
        public TemplateLeftSideMenu(IElement leftSideMenu, int contentWidth, int anchorLineWidth) {
            LeftSideMenu = leftSideMenu;
            ContentWidth = contentWidth;
            AnchorLineWidth = anchorLineWidth;
        }

        public abstract Tag WriteMaterial(Context context, IMaterial material);

        public override void ModifyBody(Tag x, Context context, IMaterial material) {
            base.ModifyBody(x, context, material);

            x.Add(new Behaviour("TemplateLeftSideMenu", ContentWidth, AnchorLineWidth).Generate(context));

            //x.Add(new TemplateLeftSideMenuBehaviour(ContentWidth, AnchorLineWidth).Generate(context));

            x.Add((LeftSideMenu.Generate(context) as Tag)
                .AddClasses("LeftSideMenu")
                .Attribute("id", "LeftSideMenu")
                );

            x.Add(WriteMaterial(context, material)
                .AddClasses("Content")
                .Attribute("id", "Content")
                );

            /*x.AppendChild(LeftSideMenu.Generate(context).Single().Do(x => {
                x.Id = "LeftSideMenu";
                x.AddClass("LeftSideMenu");

            }));

            x.AppendChild(WriteMaterial(context, material).Do(x => {
                x.Id = "Content";
                x.AddClass("Content");
            }));*/

        }
    }

}