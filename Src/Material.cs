using static Csml.Utils.Static;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public /*sealed*/ class Material : Material<Material> {
        public Material(string title, Image titleImage, FormattableString description
            ): base(title, titleImage, new Paragraph(description)) {
        }
        public Material(string title, Image titleImage, Paragraph description
            ) : base(title, titleImage, description) {
        }
    }

    public interface IMaterial {
        public string Title { get; }
        public Image TitleImage { get; set; }
    }
    

    public class Material<T> : Collection<T>, IMaterial, IPage where T : Material<T> {
        protected string UserDefinedTitle = null;
        public string Title {
            get {
                if (UserDefinedTitle != null)
                    return UserDefinedTitle;
                else {
                    var n = NameWithoutLanguage;
                    if (n == null) throw new Exception("TODO: Log.Error");
                    return n.Replace("_", " ").Trim(' ');
                }
            }
        }
        public Image TitleImage { get; set; }
        public Paragraph Description;        

        protected Material(string title, Image titleImage, Paragraph description)  {
            UserDefinedTitle = title;
            TitleImage = titleImage;
            Description = description;
        }

        public override string ToString() {
            return Title;
        }


        public void Create(Context patentContext) {
            var context = patentContext.Copy();

            Language pageLanguage = Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];


            context.SubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);

            var outputPath = Path.Combine(context.OutputDirectory, $"{NameWithoutLanguage}_{context.Language}.html");

            context.BeginPage(page=> {
                var html = page.DocumentNode.Element("html");
                html.SetAttributeValue("lang", pageLanguage.Name);

                var head = html.Element("head");
                head.Add($"<link rel = \"stylesheet\" href=\"{context.BaseUri}/Css/main.css\">");
                head.Add("<meta charset=\"utf-8\">");

                head.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");

                head.Add($"<title>{Title}</title>");


                var body = html.Element("body");
                body.Do(x => {
                    x.Add("<div>", "material").Do(x => {
                        x.Add("<div>", "header").Do(x => {
                            x.Add($"<h1>", "title").Add(Title);
                            if (TitleImage != null) {
                                x.Add(TitleImage.Generate(context));
                            }
                            x.Add("<div>", "paragraph")
                                .Add(Description.Generate(context));
                        });
                        x.Add(base.Generate(context));
                    });
                });

                })
                .EndPage(outputPath);
        }

        public Uri GetUriRelativeToRoot(Context context) {
            var thisSubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);
            Uri uri = new Uri(context.BaseUri, Path.Combine(thisSubDirectory, $"{NameWithoutLanguage}_{context.Language}.html"));
            return uri;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var translated = Translations.FirstOrDefault(x => x.Language == context.Language) ?? this;
            yield return HtmlNode.CreateNode("<a>").Do(x => {
                x.AddClass("text");
                x.SetAttributeValue("href", GetUriRelativeToRoot(context).ToString());
                x.Add(translated.Title);
                x.SetAttributeValue("title", translated.Description.ToString());
            });

            //yield return HtmlNode.CreateNode($"<a class=\"text\" href=\"{GetUriRelativeToRoot(context)}\">{translated.Title}</a>");
        }



    }
}