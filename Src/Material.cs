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
        string Title { get; }
        Image TitleImage { get; }
        IElement Description { get; }
        IElement Content { get; }
        string GetPath(Context context);
        string GetUri(Context context);
    }
    

    public class Material<T> : Collection<T>, IMaterial where T : Material<T> {
        
        IElement IMaterial.Description => Description;
        IElement IMaterial.Content => new LazyCollection(Elements);

        protected Template Template;
        //public static Template DefaultTemplate { get; set; }

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

        public string GetPath(Context context) => Path.Combine(PropertyPath, $"{NameWithoutLanguage}_{context.Language}.html");


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

        


        public string GetUri(Context context) {
            Uri uri = new Uri(context.BaseUri, GetPath(context));
            return uri.ToString();
        }

        /*public T SelectTranslation(Context context) {
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
        }*/

        bool loop = false;//TODO: delete 
        public override IEnumerable<HtmlNode> Generate(Context context) {
            //var translated = SelectTranslation(context);

            var planeDescription = "";
            if (!loop) {
                loop = true;
                planeDescription = string.Join("", Description.Generate(context).Select(x => x.InnerText));
                loop = false;
            }


            yield return HtmlNode.CreateNode("<a>").Do(x => {
                x.AddClass("text");
                x.SetAttributeValue("href", GetUri(context));
                x.InnerHtml = Title;
                x.SetAttributeValue("title", planeDescription);
            });

            //yield return HtmlNode.CreateNode($"<a class=\"text\" href=\"{GetUriRelativeToRoot(context)}\">{translated.Title}</a>");
        }



    }
}