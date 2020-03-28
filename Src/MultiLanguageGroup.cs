using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Csml {

    /*public class ReferenceContainer: IElement {
        private Dictionary<Language, IElement> translations;
        private Dictionary<Language, IElement> Translations {
            get {
                if (translations == null) translations = new Dictionary<Language, IElement>();
                return translations;
            }
        }

        public ReferenceContainer(string title) {
            Title = title;
        }

        public object GetTranslation(Language language) {
            return Translations[language];
        }
        public void AddTranslation(Language language, IElement translatable) {
            Translations.Add(language, translatable);
        }
        public string Title { get; set; }

        public IEnumerable<HtmlNode> Generate(Context context) {
            var translation = translations[context.Language];
            if (translation == null) {
                yield return HtmlNode.CreateNode($"<span>{Title}</span>");
            } else {
                if (translation is IPage) {
                    var uri = (translation as IPage).GetUriRelativeToRoot(context);
                    yield return HtmlNode.CreateNode($"<a href=\"{uri}\">{Title}</a>");
                    yield break;
                }
                if (translation is IElement) {
                    foreach (var e in translation.Generate(context)) {
                        yield return e;
                    }
                    yield break;
                }
                
                
                yield return HtmlNode.CreateNode($"<span> error ref to {Title}</span>");
                               
            }            
        }

    }*/

    public class MultiLanguageGroup : Element<MultiLanguageGroup> {
        public string Title => PropertyName.Replace("_", " ").Trim(' ');
        //private Dictionary<Language, IElement> Translations = new Dictionary<Language, IElement>();

        public override void AfterInitialization(object parent, string propertyName, PropertyInfo propertyInfo) {
            if (parent == null) {
                Log.Error.OnObject(this, $"static {nameof(MultiLanguageGroup)} not allowed", ErrorCode.StaticNotAllowed);
            }
            base.AfterInitialization(parent, propertyName, propertyInfo);
        }



        public override IEnumerable<HtmlNode> Generate(Context context) {
            var properties = PropertyInfo.DeclaringType
                .GetPropertiesAll();
            
            var page = properties
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IPage))).Select(x => x.GetValue(Parent) as IPage)
                .FirstOrDefault(x=>x.NameWithoutLanguage == NameWithoutLanguage);
            if (page != null) {
                foreach (var i in page.Generate(context)) {
                    yield return i;
                }
                yield break;
            }

            var elements = properties
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IElement)))
                .Select(x => x.GetValue(Parent) as IElement)
                .Where(x => x.NameWithoutLanguage == NameWithoutLanguage);
            var element = elements.FirstOrDefault(x => x.Language == context.Language);
            foreach (var l in Language.All) {
                if (element != null) break;
                element = elements.FirstOrDefault(x => x.Language == l);
            }
            if (element != null) {
                foreach (var i in element.Generate(context)) {
                    yield return i;
                }
            }
        }        
    }


 /*   public class MultiLanguageGroup<T> where T : MultiLanguageGroup<T> {       

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
                if (name == l.Name) return l;
                if (name.EndsWith('_' + l.Name)) return l;
            }
            return null;
        }
        
        public static void CollectTranslations() {
            var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var f in fields) {
                var l = GetLanguageByName(f.Name);
                if (l != null) {
                    var value = f.GetValue(null);
                    if (value is IElement) {
                        Reference.AddTranslation(l, value as IElement);
                        if (value is IInfo) {
                            var info = value as IInfo;
                            info.Language = l;
                            info.Name = $"{typeof(T).Name}.{f.Name}";
                        }
                    } else { 
                        //TODO: error
                    
                    }
                    

                    
                }
            }
        }


    }*/

}