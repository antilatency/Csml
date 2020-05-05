using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public class TemplateRegularMaterialBehaviour : Behaviour {
        public TemplateRegularMaterialBehaviour() : base("TemplateRegularMaterial") { }
    };

    public class TemplateRegularMaterial : Template {
        private IElement LeftSideMenu;
        private List<IElement> AdditionalBodyElements = new List<IElement>();
        public TemplateRegularMaterial(IElement leftSideMenu) {
            LeftSideMenu = leftSideMenu;
        }

        public virtual TemplateRegularMaterial Add(IElement element) {
            AdditionalBodyElements.Add(element);
            return this;
        }

        public TemplateRegularMaterial this[IElement element] { get => Add(element); }

        public static HtmlNode WriteMaterial(Context context, IMaterial material) {
            return HtmlNode.CreateNode("<div>").Do(x=> {
                x.Add("<div>", "header").Do(x => {
                    x.Add($"<h1>", "title").Add(material.Title);
                    if (material.TitleImage != null) {
                        x.Add(material.TitleImage.Generate(context));
                    }
                    x.Add(material.Description.Generate(context));
                });
                x.Add(material.Content.Generate(context));
            });
        }

        public override void ModifyBody(HtmlNode x, Context context, IMaterial material) {
            base.ModifyBody(x, context, material);

            x.Add(new TemplateRegularMaterialBehaviour().Generate(context));

            x.AppendChild(LeftSideMenu.Generate(context).Single().Do(x => {
                x.Id = "LeftSideMenu";
                x.AddClass("LeftSideMenu");

            }));

            x.AppendChild(WriteMaterial(context, material).Do(x => {
                x.Id = "Content";
                x.AddClass("Content");
            }));

            /*x.Add("<div>", "material").Do(x => {
                x.Add("<div>", "header").Do(x => {
                    x.Add($"<h1>", "title").Add(material.Title);
                    if (material.TitleImage != null) {
                        x.Add(material.TitleImage.Generate(context));
                    }
                    x.Add(material.Description.Generate(context));
                });
                x.Add(material.Content.Generate(context));
            });*/           


            foreach (var e in AdditionalBodyElements) {
                x.Add(e.Generate(context));
            }
        }
    }
}