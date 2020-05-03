using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Csml {

    public interface ILanguageSelector<I> {
        public I this[Language language] {
            get;
        }
    }

    public class LanguageSelector : LanguageSelector<LanguageSelector, IElement> {
    }

    public class LanguageSelector<I> : LanguageSelector<LanguageSelector<I>, I> where I : IElement {    
    }


    public class LanguageSelector<T,I> : Element<T>, ILanguageSelector<I> where I:IElement where T :LanguageSelector<T, I> {
        private Dictionary<Language, I> Translations;
        
        public LanguageSelector() {
            Translations = new Dictionary<Language, I>();
        }

        public LanguageSelector(params KeyValuePair<Language, I>[] translations) {
            Translations = new Dictionary<Language, I>(translations);
        }        
        

        public override void AfterInitialization(PropertyInfo propertyInfo) {
            base.AfterInitialization(propertyInfo);

            var properties = PropertyInfo.DeclaringType.GetPropertiesAll();

            var elements = properties
                .Where(x => x.GetGetMethod(true).IsStatic)
                .Where(x => x.PropertyType.ImplementsInterface(typeof(I))).Select(x => (I)x.GetValue(null))
                .Where(x => x.NameWithoutLanguage == NameWithoutLanguage)
                .Where(x => x.Language!=null);

            foreach (var e in elements) {
                Translations.Add(e.Language, e);
            }
            
        }

        public I this[Language language] {
            get {
                if (Translations.ContainsKey(language)) {
                    return Translations[language];
                }
                foreach (var l in Language.All) {
                    if (Translations.ContainsKey(l)) {
                        return Translations[l];
                    }
                }
                return default;
            }
        }
        public override IEnumerable<HtmlNode> Generate(Context context) {
            var translated = this[context.Language];
            if (typeof(I) == typeof(IMaterial)) {
                if (translated == null) {
                    return new[]{
                        CsmlPredefined.NoTarget.Generate(context).Single().Do(x => {
                            x.AddClass("no-target");
                            x.InnerHtml = Title;
                        })
                    };
                }
            }            
            return this[context.Language].Generate(context);
        }

        private string Title {
            get {
                var n = PropertyName;
                if (n == null) throw new Exception("TODO: Log.Error");
                return n.Replace("_", " ").Trim(' ');                
            }
        }

    }

    /*public class MultiLanguageGroup : Element<MultiLanguageGroup> {
        protected string UserDefinedTitle = null;
        public string Title {
            get {
                if (UserDefinedTitle != null)
                    return UserDefinedTitle;
                else {
                    var n = PropertyName;
                    if (n == null) throw new Exception("TODO: Log.Error");
                    return n.Replace("_", " ").Trim(' ');
                }
            }
        }

        public MultiLanguageGroup(string title = null) {
            UserDefinedTitle = title;
        }

        //public string Title => PropertyName.Replace("_", " ").Trim(' ');
        //private Dictionary<Language, IElement> Translations = new Dictionary<Language, IElement>();

        public override void AfterInitialization(PropertyInfo propertyInfo) {
            base.AfterInitialization(propertyInfo);
        }

        public override string ToString() {
            return Title;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var properties = PropertyInfo.DeclaringType
                .GetPropertiesAll();
            
            var page = properties
                .Where(x=>x.GetGetMethod(true).IsStatic)
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IPage))).Select(x => x.GetValue(null) as IPage)
                .FirstOrDefault(x=>x.NameWithoutLanguage == NameWithoutLanguage);
            if (page != null) {
                foreach (var i in (page as IElement).Generate(context)) {
                    yield return i;
                }
                yield break;
            }

            var elements = properties
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IElement)))
                .Select(x => x.GetValue(null) as IElement)
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
                yield break;
            }


            //No target. 
            yield return CsmlPredefined.NoTarget.Generate(context).Single().Do(x => {
                x.AddClass("no-target");
                x.InnerHtml = Title;
            });
            
        }        
    }*/


}