using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {

    public class Template<T> : IPageTemplate where T: Template<T> {
        readonly List<IElement> AdditionalBodyElements = new List<IElement>();
        public virtual T Add(IElement element) {
            AdditionalBodyElements.Add(element);
            return this as T;
        }
        public T this[IElement element] { get => Add(element); }

        public virtual void ModifyHead(HtmlNode x, Context context, IMaterial material) {
            x.Add($"<title>{CsmlApplication.PageTitlePrefix + material.Title}</title>");

            x.Add($"<link rel = \"stylesheet\" href=\"{CsmlApplication.WwwCssUri}\">");
            x.Add($"<script src=\"{CsmlApplication.WwwJsUri}\">");
            x.Add("<meta charset=\"utf-8\">");
            x.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");

            x.Add($"<meta property=\"og:title\" content=\"{material.Title}\">");
           // x.Add($"<meta property=\"og:type\" content=\"article\">");
            var materialDescription = string.Join("", material.Description.Generate(context).Select(x => x.InnerText));
            x.Add($"<meta property=\"og:description\" content=\"{materialDescription}\">");

            if (material.TitleImage != null) {
                x.Add($"<meta property=\"og:image\" content=\"{material.TitleImage.GetUri()}\">");
            }
            
            x.Add($"<meta property=\"og:url\" content=\"{material.GetUri(context.Language)}\">");

            x.Add($"<meta property=\"twitter:card\" content=\"summary\">");
        }

        public virtual void ModifyBody(HtmlNode x, Context context, IMaterial material) {
            x.AddClass(GetType().Name);
            
        }

        public void Generate(Context context, IMaterial material) {

            Language pageLanguage = material.Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];

            var outputPath = Path.Combine(CsmlApplication.WwwRootDirectory, material.GetPath(context.Language));
            context.CurrentMaterial = material;
            context.BeginPage();
            context.CurrentHtmlDocument.DocumentNode.Do(x => {
                x.Element("html").Do(x => {
                    x.SetAttributeValue("lang", pageLanguage.Name);
                    x.Element("head").Do(x => ModifyHead(x, context, material));
                    x.Element("body").Do(x => {
                        ModifyBody(x, context, material);
                        foreach (var e in AdditionalBodyElements) {
                            x.Add(e.Generate(context));
                        }
                    });
                });
            });
            context.EndPage(outputPath);
        }
    }
}