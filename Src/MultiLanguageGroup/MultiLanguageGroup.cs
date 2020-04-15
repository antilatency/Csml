using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;

namespace Csml {


    public class MultiLanguageGroup : Element<MultiLanguageGroup> {
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
            /*if (parent == null) {
                Log.Error.OnObject(this, $"static {nameof(MultiLanguageGroup)} not allowed", ErrorCode.StaticNotAllowed);
            }*/
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
                foreach (var i in page.Generate(context)) {
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
    }


}