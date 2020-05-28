using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public sealed class Material : Material<Material> {
        

        public Material(string title, Image titleImage, FormattableString description
            ) : base(title, titleImage, new Paragraph(description)) {
        }
        public Material(string title, Image titleImage, Paragraph description
            ) : base(title, titleImage, description) {
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

        //protected Template Template;
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

        public string GetPath(Context context) {
            if (NameWithoutLanguage == "Material") {
                return $"{PropertyPath}_{context.Language}.html";
            }
            return Path.Combine(PropertyPath, $"{NameWithoutLanguage}_{context.Language}.html");
        }

        public Paragraph Description;        

        protected Material(string title, Image titleImage, Paragraph description)  {
            UserDefinedTitle = title;
            TitleImage = titleImage;
            Description = description;
        }

        public override string ToString() {
            return Title;
        }

        


        public string GetUri(Context context) {
            Uri uri = new Uri(Application.BaseUri, GetPath(context));
            return uri.ToString();
        }


        bool loop = false;//TODO: delete 
        public override IEnumerable<HtmlNode> Generate(Context context) {
            //var translated = SelectTranslation(context);

            var planeDescription = "";
            if (!loop) {
                loop = true;
                var it = Description.Generate(context).Select(x => x.InnerText);
                planeDescription = string.Join("", Description.Generate(context).Select(x => x.InnerText));
                loop = false;
            }


            yield return HtmlNode.CreateNode(context.AForbidden ? "<span>" : "<a>").Do(x => {
                x.AddClass("text");
                if (!context.AForbidden) {
                    x.SetAttributeValue("href", GetUri(context));
                    x.SetAttributeValue("title", planeDescription);
                }
                x.InnerHtml = Title;                
            });

            //yield return HtmlNode.CreateNode($"<a class=\"text\" href=\"{GetUriRelativeToRoot(context)}\">{translated.Title}</a>");
        }



    }
}