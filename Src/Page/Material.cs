using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Htmlilka;

namespace Csml {

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

        public IImage TitleImage { get; set; }

        public Paragraph Description;

        public static string GetPath(Language language, PropertyInfo materialProperty) {
            if (GetNameWithoutLanguage(materialProperty) == "Material") {
                return $"{GetPropertyPath(materialProperty)}_{language}.html";
            }
            return Path.Combine(GetPropertyPath(materialProperty), $"{GetNameWithoutLanguage(materialProperty)}_{language}.html");
        }

        public string GetPath(Language language) {
            return GetPath(language, PropertyInfo);
        }

        public Material(string title, IImage titleImage, FormattableString description) 
            : this(title, titleImage, new Paragraph(description))
        { }

        public Material(string title, IImage titleImage, Paragraph description)  {
            UserDefinedTitle = title;
            TitleImage = titleImage;
            Description = description;
        }

        public override string ToString() {
            return Title;
        }

        public static string GetUri(Language language, PropertyInfo materialProperty) {
            Uri uri = new Uri(CsmlApplication.WwwRootUri, GetPath(language, materialProperty));
            return uri.ToString();
        }

        public string GetUri(Language language) {
            return GetUri(language, PropertyInfo);
        }

        bool loop = false;//TODO: delete 
        public override Node Generate(Context context) {
            //var translated = SelectTranslation(context);

            var planeDescription = "";
            if (!loop) {
                loop = true;
                //var it = Description.Generate(context).Select(x => x.InnerText);
                var descriptionNode = Description.Generate(context);
                var stringBuilder = new StringBuilder();
                descriptionNode.WritePlaneText(stringBuilder);
                planeDescription = stringBuilder.ToString() ;
                loop = false;
            }

            

            var result = new Tag(context.AForbidden ? "span" : "a")
                    .AddClasses("Text")
                    .AddText(Title);


            if (!context.AForbidden) {
                var href = GetUri(context.Language);
                if (context.FormatString != null) {
                    href += "#" + context.FormatString;
                }
                result.Attribute("href", href);
                result.Attribute("title", planeDescription);
            }

            return result;
        }
    }
}