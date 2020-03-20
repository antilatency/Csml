using static Csml.Utils.Static;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
        
        

        protected Material(string title, object titleImage, Text description)  {
            Title = title;
            this.TitleImage = titleImage;
            this.description = description;
        }

        public override string ToString() {
            return Title;
        }



        public void Create(Context patentContext) {
            var context = patentContext.Copy();
            context.Language = Language;
            context.SubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);

            var outputPath = Path.Combine(context.OutputDirectory, Name + ".html");

            context.BeginPage(page=> {
                var html = page.DocumentNode.Element("html");
                html.SetAttributeValue("lang", Language.name);

                var head = html.Element("head");
                head.Add($"<link rel = \"stylesheet\" href=\"{context.BaseUri}/Css/main.css\">");


                head.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");

                head.Add($"<title>{Title}</title>");


                var body = html.Element("body");
                var material = body.Add($"<div {Class("material")}> ");

                var title = material.Add($"<h1 {Class("material-title")}>{Title}</h1>");

                description.Generate(context).ForEach(x => material.AppendChild(x));
                    

                base.Generate(context).ForEach(x => material.AppendChild(x));


                })
                .EndPage(outputPath);
        }

    }
}