using System.Reflection;
using System.Diagnostics;
using System;
using System.IO;
using HtmlAgilityPack;
using System.Collections.Generic;

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
        public void Create(Context context);
        public Uri GetUriRelativeToRoot(Context context);
    }

    public interface IElement {
        public IEnumerable<HtmlNode> Generate(Context context);
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

        public virtual IEnumerable<HtmlNode> Generate(Context context) {
            return null;
        }

        public Uri GetUriRelativeToRoot(Context context) {
            var thisSubDirectory = context.GetSubDirectoryFromSourceAbsoluteFilePath(CallerSourceFilePath);
            Uri uri = new Uri(context.BaseUri, Path.Combine(thisSubDirectory, Name + ".html"));
            return uri;
        }


    }

}