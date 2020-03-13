using System;
using System.Diagnostics;

namespace Csml {

    public static class Preprocessor {

        public static void Process<R>() {
            try {
                Translatable.AssignLanguages<R>();

                Term.SetTermNames<R>();

                CheckForInstanceFields<R>();
            }
            catch (Exception e){
                Log.Error.OnException(GetDeepestException(e));
            }
        }



        static Exception GetDeepestException(Exception e) {
            if (e.InnerException == null) {
                return e;
            } else {
                return GetDeepestException(e.InnerException);
            }
        }

        static void CheckForInstanceFields<T>(){
            //typeof(T).GetFields()
        }
    }
}