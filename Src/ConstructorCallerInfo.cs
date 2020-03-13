using System.Reflection;
using System.Diagnostics;
using System;

namespace Csml {
    public interface ICallerInfo {
        public string CallerSourceFilePath { get; }
        public int CallerSourceLineNumber { get; }
    }

    public interface IFinal {
        public Type ImplementerType { get; }
    }


    public class CallerInfo<T> : ICallerInfo, IFinal {
        public Type ImplementerType => typeof(T);

        public string CallerSourceFilePath { get; set; }
        public int CallerSourceLineNumber { get; set; }

        private bool IsConstructorOfT(MethodBase method) {
            if (method.Name != ".ctor") return false;
            return method.DeclaringType == typeof(T);
        }
        
        public CallerInfo() {
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

        /*public void CaptureCallerInfo() {
            

        }

        public void SetCallerInfo(string memberName,string sourceFilePath,int sourceLineNumber) {
            callerMemberName = memberName;
            callerSourceFilePath = sourceFilePath;
            callerSourceLineNumber = sourceLineNumber;
        }*/
    }

}