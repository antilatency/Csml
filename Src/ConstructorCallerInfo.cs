using System.Reflection;
using System.Diagnostics;
namespace Csml {
    public interface ICallerInfo {
        public string callerSourceFilePath { get; }
        public int callerSourceLineNumber { get; }
    }

    public class CallerInfo<T> : ICallerInfo {
        public string callerSourceFilePath { get; set; }
        public int callerSourceLineNumber { get; set; }

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
                        callerSourceFilePath = fprew.GetFileName();
                        callerSourceLineNumber = fprew.GetFileLineNumber();
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