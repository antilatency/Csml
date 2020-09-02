using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Htmlilka;

namespace Csml {

    public class Template<T> : IPageTemplate where T: Template<T> {
        readonly List<IElement> AdditionalBodyElements = new List<IElement>();
        public virtual T Add(IElement element) {
            AdditionalBodyElements.Add(element);
            return this as T;
        }
        public T this[IElement element] { get => Add(element); }

        public virtual void ModifyHead(Tag x, Context context, IMaterial material) {
            x.AddTag("title", x => {
                x.AddText(CsmlApplication.PageTitlePrefix + material.Title);
            });

            x.AddVoidTag("link", x => {
                x.Attribute("rel", "stylesheet");
                x.Attribute("href", CsmlApplication.WwwCssUri.ToString());
            });

            x.AddTag("script", a => {
                a.Attribute("src", CsmlApplication.WwwJsUri.ToString());
            });

            x.AddMeta(x => {
                x.Attribute("charset", "utf-8");
            });

            x.AddMeta(x => {
                x.Attribute("name", "viewport");
                x.Attribute("content", "width=device-width, initial-scale=1, shrink-to-fit=yes");
            });

            x.AddMeta(x => {
                x.Attribute("property", "og:title");
                x.Attribute("content", material.Title);
            });

            // x.Add($"<meta property=\"og:type\" content=\"article\">");

            x.AddMeta(x => {
                x.Attribute("property", "og:title");
                x.Attribute("content", material.Title);
            });

            var materialDescription = material.Description.Generate(context).GetPlaneText();
            x.AddMeta(x => {
                x.Attribute("property", "og:description");
                x.Attribute("content", materialDescription);
            });

            if (material.TitleImage != null) {
                x.AddMeta(x => {
                    x.Attribute("property", "og:image");
                    x.Attribute("content", material.TitleImage.GetUri().ToString());
                });
            }


            x.AddMeta(x => {
                x.Attribute("property", "og:url");
                x.Attribute("content", material.GetUri(context.Language));
            });

            x.AddMeta(x => {
                x.Attribute("property", "twitter:card");
                x.Attribute("content", "summary");
            });


            /*
                x.Add($"<script src=\"{CsmlApplication.WwwJsUri}\">");
            x.Add("<meta charset=\"utf-8\">");
            x.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");

            x.Add($"<meta property=\"og:title\" content=\"{material.Title}\">");
            

            var materialDescription = material.Description.Generate(context);

            var materialDescription = string.Join("", material.Description.Generate(context).Select(x => x.InnerText));
            x.Add($"<meta property=\"og:description\" content=\"{materialDescription}\">");

            if (material.TitleImage != null) {
                x.Add($"<meta property=\"og:image\" content=\"{material.TitleImage.GetUri()}\">");
            }

            x.Add($"<meta property=\"og:url\" content=\"{material.GetUri(context.Language)}\">");

            x.Add($"<meta property=\"twitter:card\" content=\"summary\">");*/
        }

        public virtual void ModifyBody(Tag x, Context context, IMaterial material) {
            x.AddClasses(GetType().Name);
            
        }


        int NextCapacity = 4096;

        private void SaveHtml(string path, Tag tag) {
            StringBuilder stringBuilder = new StringBuilder(NextCapacity*2);
            tag.WriteHtml(stringBuilder);
            Utils.CreateDirectory(Path.GetDirectoryName(path));
            var result = stringBuilder.ToString();

            NextCapacity = (result.Length + NextCapacity) / 2;

            File.WriteAllText(path, result);
        }

        public void Generate(Context context, IMaterial material) {
            var outputPath = Path.Combine(CsmlApplication.WwwRootDirectory, material.GetPath(context.Language));
            SaveHtml(outputPath, GenerateDom(context, material));
        }

        public Tag GenerateDom(Context context, IMaterial material) {
            Language pageLanguage = material.Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];
            
            context.CurrentMaterial = material;

            var head = new Tag("head");
            ModifyHead(head, context, material);

            var body = new Tag("body");
            ModifyBody(body, context, material);

            foreach (var e in AdditionalBodyElements) {
                body.Add(e.Generate(context));
            }

            var page = new Tag(null)
                .AddVoidTag("!DOCTYPE", a => a.Attribute("html"))
                .AddTag("html", a => {
                    a.Attribute("lang", pageLanguage.Name);
                    a.Add(head);
                    a.Add(body);
                });
            return page;
        }
    }
}