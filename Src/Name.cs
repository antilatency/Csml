using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Csml {

    public class ReferenceContainer: IElement {
        private Dictionary<Language, object> translations;
        private Dictionary<Language, object> Translations {
            get {
                if (translations == null) translations = new Dictionary<Language, object>();
                return translations;
            }
        }

        public ReferenceContainer(string title) {
            Title = title;
        }

        public object GetTranslation(Language language) {
            return Translations[language];
        }
        public void AddTranslation(Language language, object translatable) {
            Translations.Add(language, translatable);
        }
        public string Title { get; set; }

        public HtmlNode Generate() {
            return HtmlNode.CreateNode($"<a href=\"https://www.antilatency.com\">{Title}</a>");
        }
    }




    public class Name<T> where T : Name<T> {       

        public static ReferenceContainer referenceContainer;
        public static ReferenceContainer Reference {
            get {
                if (referenceContainer == null) {
                    referenceContainer = new ReferenceContainer(Title);
                }
                return referenceContainer;
            }
        }
        public static string Title => typeof(T).Name.Replace('_', ' ').Trim(' ');

        private static Language GetLanguageByName(string name) {
            var languages = typeof(Language)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(Language)).Select(x=>(Language)x.GetValue(null));

            foreach (var l in languages) {
                if (name == l.name) return l;
                if (name.EndsWith('_' + l.name)) return l;
            }
            return null;
        }
        
        public static void CollectTranslations() {
            var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var f in fields) {
                var l = GetLanguageByName(f.Name);
                if (l != null) {
                    var value = f.GetValue(null);
                    Reference.AddTranslation(l,value);
                    if (value is IInfo) {
                        var info = value as IInfo;
                        info.Language = l;
                        info.Name = $"{typeof(T).Name}.{f.Name}";
                    }
                }
            }
        }


    }

}