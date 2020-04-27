using System.Reflection;
using System.Diagnostics;
using System;
using System.IO;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public interface IInfo {
        public string CallerSourceFilePath { get; }
        public int CallerSourceLineNumber { get; }
        //public string Name { get; set; }//Auto assign by Engine
        //public Language Language { get; }//Auto assign by Engine
    }

    public interface IFinal {
        public Type ImplementerType { get; }
    }

    public interface IPage : IElement {
        public void Create(Context context);
        public Uri GetUriRelativeToRoot(Context context);
        
    }

    public interface IElement {
        public IEnumerable<HtmlNode> Generate(Context context);
        public string NameWithoutLanguage { get; }
        public Language Language { get; }
        public List<IElement> Translations { get; }
    }

    public class Element : Element<Element> {
        private Func<Context,HtmlNode> Generator { get; set; }
        public Element(Func<Context, HtmlNode> generator) {
            Generator = generator;
        }
        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return Generator(context);
        }
    }


    public class Modify : Element<Modify> {
        private IElement Element;
        Func<Context, IEnumerable<HtmlNode>> Modifier;
        //public Func<Context, IEnumerable<HtmlNode>> ModifierClone => (Func<Context, IEnumerable<HtmlNode>>)Modifier.Clone();
        
        public Modify(FormattableString formattableString): this(new Text(formattableString)){
            
        }

        public Modify(IElement element = null) {
            if (element == null) {
                Element = new Element(context => HtmlNode.CreateNode("<span>"));
            } else {
                Element = element;
            }            
            Modifier = (context) => Element.Generate(context);
        }

        public Modify ContentReplace(FormattableString replacement) {
            return ContentReplace(new Text(replacement));
        }
        public Modify ContentReplace(IElement replacement) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Visit(x => {
                    x.InnerHtml = "";
                    x.Add(replacement.Generate(context));
                });
            return this;
        }
        public HtmlNode WrapTextToSpan(HtmlNode x) {
            if (x.Name == "#text") {
                return HtmlNode.CreateNode($"<span>{x.InnerText}</span>");
            }
            return x;
        }

        public Modify AddClasses(params string[] classes) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Select(x => {
                    x = WrapTextToSpan(x);                    
                    foreach (var c in classes)
                        x.AddClass(c);
                    return x;
                });
            return this;
        }

        public Modify SetAttributeValue(string name, string value) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Select(x => {
                    x = WrapTextToSpan(x);
                    x.SetAttributeValue(name, value);
                    return x;
                });
            return this;
        }

        public Modify WrapIfMany(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                var previous = prevModifier(context);
                var count = previous.Count();
                if (count == 1) return previous;
                return new HtmlNode[] {
                    HtmlNode.CreateNode($"<{tag}>").Add(previous)
                };
            };
            return this;
        }

        public Modify Wrap(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                return new HtmlNode[] {
                    HtmlNode.CreateNode($"<{tag}>").Add(prevModifier(context))
                };
            };
            return this;
        }

        public Modify Tag(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => prevModifier(context).Visit(x => {
                x.Name = tag;
                }
                ) ;
            return this;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var generated = Modifier(context);
            foreach (var g in generated) yield return g;
        }

    }

    public class ContentReplace : Element<ContentReplace> {
        private IElement Element;
        private IElement Replacement;
        public ContentReplace(IElement element, FormattableString replacement) {
            Element = element;
            Replacement = new Text(replacement);
        }
        public ContentReplace(IElement element, IElement replacement) {
            Element = element;
            Replacement = replacement;
        }
        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return Element.Generate(context).Single().Do(x=> {
                x.InnerHtml = "";
                x.Add(Replacement.Generate(context));
            });
        }
        public override string ToString() {
            return Replacement.ToString();
        }
    }



    public class Element<T> : GetOnce.IStaticPropertyInitializer, IElement, IInfo, IFinal where T : Element<T> {
        public Type ImplementerType => typeof(T);
        public string CallerSourceFilePath { get; set; }
        public int CallerSourceLineNumber { get; set; }

        //public string Name { get; set; }//Auto assign by Engine

        protected string PropertyName => PropertyInfo?.Name;
        protected PropertyInfo PropertyInfo;

        public virtual void AfterInitialization(PropertyInfo propertyInfo) {
            PropertyInfo = propertyInfo;            
        }

        public string NameWithoutLanguage {
            get {
                
                if (PropertyName == null) return null;
                foreach (var l in Language.All) {
                    var languageSuffix = "_" + l.Name;
                    if (PropertyName.EndsWith(languageSuffix))
                        return PropertyName.Substring(0, PropertyName.Length - languageSuffix.Length);
                }
                return PropertyName;
            }
        }

        public Language Language {
            get {
                foreach (var l in Language.All) {
                    var languageSuffix = "_" + l.Name;
                    if (PropertyName.EndsWith(languageSuffix))
                        return l;
                }
                return null;
            }
        }

        List<IElement> IElement.Translations => Translations?.Select(x => x as IElement).ToList();

        public List<T> Translations {
            get {
                if (Language == null) return null;
                var properties = PropertyInfo.DeclaringType.GetProperties(
                    BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.Static);

                var result = new List<T>();
                foreach (var p in properties) {
                    if (p.PropertyType == typeof(T)) {
                        var v = p.GetValue(null);
                        if ((v is T) && (v!=this)) {
                            var vt = v as T;
                            if (vt.NameWithoutLanguage == NameWithoutLanguage) {
                                result.Add(vt);
                            }
                        }
                    }
                }
                return (result.Count == 0)?null:result;
            }
        }







        private bool IsConstructorOfT(MethodBase method) {
            if (method.Name != ".ctor") return false;
            return method.DeclaringType == typeof(T);
        }
        
        public Element() {
            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++) {
                var f = st.GetFrame(i);
                if (IsConstructorOfT(f.GetMethod())) {
                    var fprew = st.GetFrame(i+1);
                    if (fprew != null) {
                        CallerSourceFilePath = fprew.GetFileName();
                        CallerSourceLineNumber = fprew.GetFileLineNumber();
                        break;
                    }                    
                }
            }
        }

        public virtual IEnumerable<HtmlNode> Generate(Context context) {
            yield break;
        }




        //Utils
        public string ConvertPathToAbsolute(string filePath) {
            return ConvertPathToAbsolute(filePath, CallerSourceFilePath);
        }

        public static string ConvertPathToAbsolute(string filePath, string CallerSourceFilePath) {
            if (Path.IsPathRooted(filePath)) {
                return filePath;
            } else {
                return Path.GetFullPath(filePath, Path.GetDirectoryName(CallerSourceFilePath));
            }
        }

        /*public string ReadAllText(string path) {
            File.ReadAllText
        }*/






    }

}