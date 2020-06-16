using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Csml {

    public interface ILanguageSelector<I> {
        bool HasTarget { get; }
        public I this[Language language] {
            get;
        }
        public string Title { get; }
    }

    public class LanguageSelector : LanguageSelector<LanguageSelector, IElement>, ILanguageSelector<IElement> {
    }

    public class LanguageSelector<I> : LanguageSelector<LanguageSelector<I>, I>, ILanguageSelector<I> where I : IElement {    
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

        public bool HasTarget => Translations.Count > 0;

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

        public string Title {
            get {
                var n = PropertyName;
                if (n == null) throw new Exception("TODO: Log.Error");
                return n.Replace("_", " ").Trim(' ');                
            }
        }
        public override string ToString() {
            return Title;
        }

    }
}