using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public /*sealed*/ class Material : Material<Material> {
        

        public Material(string title, Image titleImage, FormattableString description
            ) : base(null, title, titleImage, new Paragraph(description)) {
        }
        public Material(string title, Image titleImage, Paragraph description
            ) : base(null, title, titleImage, description) {
        }

        public Material(Template template, string title, Image titleImage, FormattableString description
            ): base(template, title, titleImage, new Paragraph(description)) {
        }
        public Material(Template template, string title, Image titleImage, Paragraph description
            ) : base(template, title, titleImage, description) {
        }
    }

    public interface IMaterial : IElement {
        public string Title { get; }
        public Image TitleImage { get; set; }
    }
    

    public class Material<T> : Collection<T>, IMaterial, IPage where T : Material<T> {
        protected Template Template;
        public static Template DefaultTemplate { get; set; }

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

        protected Material(Template template, string title, Image titleImage, Paragraph description)  {
            Template = template;
            UserDefinedTitle = title;
            TitleImage = titleImage;
            Description = description;
        }

        public override string ToString() {
            return Title;
        }

        public string SubDirectory => PropertyInfo.DeclaringType.FullName.Replace("+",".").Replace(".", "/");

        public void Create(Context patentContext) {
            var context = patentContext.Copy();

            Language pageLanguage = Language;
            if (pageLanguage == null) pageLanguage = Language.All[0];


            context.SubDirectory = SubDirectory;// context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);

            var outputPath = Path.Combine(context.OutputDirectory, $"{NameWithoutLanguage}_{context.Language}.html");
            context.CurrentMaterial = this;
            context.BeginPage();
            context.CurrentHtmlDocument.DocumentNode.Do(x => {
                x.Element("html").Do(x => {
                    x.SetAttributeValue("lang", pageLanguage.Name);
                    x.Element("head").Do(x => {
                        x.Add($"<link rel = \"stylesheet\" href=\"{context.BaseUri}/style.css\">");
                        x.Add("<meta charset=\"utf-8\">");
                        x.Add("<meta name = \"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=yes\">");
                        x.Add($"<title>{context.WatchPrefix+Title}</title>");
                    });
                    x.Element("body").Do(x => {
                        if ((Template ?? DefaultTemplate)?.Pre != null) x.Add((Template ?? DefaultTemplate).Pre.Generate(context));
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

                        if ((Template ?? DefaultTemplate)?.Post != null) x.Add((Template ?? DefaultTemplate).Post.Generate(context));
                    });
                });                
            });
            context.EndPage(outputPath);
            context.CurrentMaterial = null;


            /*(this as IMaterial).Translations.ForEach(
                x => Console.WriteLine( x.Generate())
                
                );*/

            /*page => {
                var html = page.DocumentNode.Element("html");
                html.SetAttributeValue("lang", pageLanguage.Name);

                var head = html.Element("head");
                


                var body = html.Element("body");
                body.Do(x => {
                    
                });

                })*/

        }

        public Uri GetUriRelativeToRoot(Context context) {
            var thisSubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);
            Uri uri = new Uri(context.BaseUri, Path.Combine(SubDirectory, $"{NameWithoutLanguage}_{context.Language}.html"));
            return uri;
        }

        public T SelectTranslation(Context context) {
            if (this.Language == context.Language) return this as T;
            var translated = Translations?.FirstOrDefault(x => x.Language == context.Language);
            if (translated != null) return translated;
            if (Translations != null) {
                foreach (var l in Language.All) {
                    translated = (l==Language)?(this as T):Translations.FirstOrDefault(x => x.Language == l);
                    if (translated != null) return translated;
                }
            }
            return this as T;
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            var translated = SelectTranslation(context);

            yield return HtmlNode.CreateNode("<a>").Do(x => {
                x.AddClass("text");
                x.SetAttributeValue("href", GetUriRelativeToRoot(context).ToString());
                x.InnerHtml = translated.Title;
                x.SetAttributeValue("title", translated.Description.ToString());
            });

            //yield return HtmlNode.CreateNode($"<a class=\"text\" href=\"{GetUriRelativeToRoot(context)}\">{translated.Title}</a>");
        }



    }
}