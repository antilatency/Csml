using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {

    public interface IMaterial : IElement {
        string Title { get; }
        Image TitleImage { get; }
        IElement Description { get; }
        IElement Content { get; }
        string GetPath(Language language);
        string GetUri(Language language);
    }
    

    public class Material : Collection<Material>, IMaterial {

        IElement IMaterial.Description => Description;
        IElement IMaterial.Content => new LazyCollection(Elements);

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

        public string GetPath(Language language) {
            if (NameWithoutLanguage == "Material") {
                return $"{PropertyPath}_{language}.html";
            }
            return Path.Combine(PropertyPath, $"{NameWithoutLanguage}_{language}.html");
        }

        public Material(string title, Image titleImage, FormattableString description) 
            : this(title, titleImage, new Paragraph(description))
        { }

        public Material(string title, Image titleImage, Paragraph description)  {
            UserDefinedTitle = title;
            TitleImage = titleImage;
            Description = description;
        }

        public override string ToString() {
            return Title;
        }

        public string GetUri(Language language) {
            Uri uri = new Uri(Application.BaseUri, GetPath(language));
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
                x.AddClass("Text");
                if (!context.AForbidden) {
                    var href = GetUri(context.Language);
                    if (context.FormatString != null) {
                        href += "#" + context.FormatString;
                    }
                    x.SetAttributeValue("href", href);
                    x.SetAttributeValue("title", planeDescription);
                }
                x.InnerHtml = Title;                
            });
        }
    }
}