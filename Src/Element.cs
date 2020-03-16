using System.Reflection;
using System.Diagnostics;
using System;

namespace Csml {
    public interface IInfo {
        public string CallerSourceFilePath { get; }
        public int CallerSourceLineNumber { get; }
        public string Name { get; set; }//Auto assign by Engine
        public Language Language { get; set; }//Auto assign by Engine
    }

    public interface IFinal {
        public Type ImplementerType { get; }
    }

    public interface IPage {
        public void Create();
        public string Url { get; set; }//Auto assign by Engine
    }

    public interface IElement {
        public HtmlAgilityPack.HtmlNode Generate();
    }

    public class Element<T> : IElement, IInfo, IFinal {
        public Type ImplementerType => typeof(T);

        public string CallerSourceFilePath { get; set; }
        public int CallerSourceLineNumber { get; set; }

        public string Name { get; set; }//Auto assign by Engine
        public Language Language { get; set; }//Auto assign by Engine

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
                    }                    
                }
            }
        }

        public virtual HtmlAgilityPack.HtmlNode Generate() {
            return null;
        }


    }

}