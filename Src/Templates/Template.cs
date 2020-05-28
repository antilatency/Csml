using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;

namespace Csml {

    public class Template<T> : ITemplate where T: Template<T> {
        readonly List<IElement> AdditionalBodyElements = new List<IElement>();
        public virtual T Add(IElement element) {
            AdditionalBodyElements.Add(element);
            return this as T;
        }
        public T this[IElement element] { get => Add(element); }

        public virtual void ModifyHead(HtmlNode x, Context context, IMaterial material) { 
            x.Add($"<link rel = \"stylesheet\" href=\"{Application.CssUri}\">");
            x.Add($"<script src=\"{Application.JsUri}\">");
            x.Add("<meta charset=\"utf-8\">");
            x.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");
            x.Add($"<title>{Application.TitlePrefix + material.Title}</title>");
        }

        public virtual void ModifyBody(HtmlNode x, Context context, IMaterial material) {
            x.AddClass(GetType().Name);
            
        }

        public void Generate(Context context, IMaterial material) {

            Language pageLanguage = material.Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];

            var outputPath = Path.Combine(Application.OutputRootDirectory, material.GetPath(context));
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