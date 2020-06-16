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
    }

    public interface IFinal {
        public Type ImplementerType { get; }
    }

    public interface IElement {
        public string NameWithoutLanguage { get; }
        public string PropertyPath { get; }
        public Language Language { get; }
        public IEnumerable<HtmlNode> Generate(Context context);        
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

    public class Element<T> : GetOnce.IStaticPropertyInitializer, IElement, IInfo, IFinal where T : Element<T> {
        public Type ImplementerType => typeof(T);
        public string CallerSourceFilePath { get; set; }
        public int CallerSourceLineNumber { get; set; }

        public string PropertyPath => PropertyInfo.DeclaringType.FullName.Replace("+", ".").Replace(".", "/");
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
    }

}