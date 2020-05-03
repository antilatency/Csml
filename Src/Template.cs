using System.IO;
using HtmlAgilityPack;

namespace Csml {
    public interface ITemplate {
        void Generate(Context context, IMaterial material);
    }

    /*public class RegularPage : Template {
        public IElement Pre { get; set; }
        public IElement Post { get; set; }
        public RegularPage(IElement additionalElements) {
            Pre = pre;
            Post = post;
        }
    }*/

    public class Template : ITemplate {
        /*public IElement Pre { get; set; }
        public IElement Post { get; set; }
        public Template(IElement pre, IElement post) {
            Pre = pre;
            Post = post;
        }*/

        public virtual void ModifyHead(HtmlNode x, Context context, IMaterial material) { 
            x.Add($"<link rel = \"stylesheet\" href=\"{context.BaseUri}/style.css\">");
            x.Add($"<script src=\"{context.BaseUri}/script.js\">");
            x.Add("<meta charset=\"utf-8\">");
            x.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");
            x.Add($"<title>{context.WatchPrefix + material.Title}</title>");
        }

        public virtual void ModifyBody(HtmlNode x, Context context, IMaterial material) {
            x.Add("<div>", "material").Do(x => {
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

        public void Generate(Context context, IMaterial material) {

            Language pageLanguage = material.Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];

            var outputPath = Path.Combine(context.OutputRootDirectory, material.GetPath(context));
            context.CurrentMaterial = material;
            context.BeginPage();
            context.CurrentHtmlDocument.DocumentNode.Do(x => {
                x.Element("html").Do(x => {
                    x.SetAttributeValue("lang", pageLanguage.Name);
                    x.Element("head").Do(x => ModifyHead(x, context, material));
                    x.Element("body").Do(x => ModifyBody(x, context, material));
                });
            });
            context.EndPage(outputPath);
        }
    }
}