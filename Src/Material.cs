using System;

namespace Csml {
    public sealed class Material : Material<Material> {
        public Material(string title, object titleImage, FormattableString description
            ): base(title, titleImage, new Text(description)) {
        }
        public Material(string title, object titleImage, Text description
            ) : base(title, titleImage, description) {
        }
    }

    public interface IMaterial {
        public string Title { get; set; }
        public object TitleImage { get; set; }
    }
    

    public class Material<T> : Collection<T>, IMaterial, IPage where T : Material<T> {
        public string Title { get; set; }
        public object TitleImage { get; set; }
        public Text description;
        
        public string Url { get; set; }

        protected Material(string title, object titleImage, Text description)  {
            Title = title;
            this.TitleImage = titleImage;
            this.description = description;
        }

        public override string ToString() {
            return Title;
        }

        public void Create() {
            Context
                .Push()
                .SetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath)
                .BeginPage($"{Name}.html",page=> {
                    var html = page.DocumentNode.Element("html");
                    html.SetAttributeValue("lang", Language.name);

                    var head = html.Element("head");
                    var body = html.Element("body");

                    var title = body.AppendChild(page.CreateElement("h1"));
                    title.AppendChild(page.CreateTextNode(Title));

                    body.AppendChild(description.Generate());

                })
                .EndPage();
            Context.Pop();
        }
    }
}