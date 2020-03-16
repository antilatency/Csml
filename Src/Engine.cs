using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csml {

    public static class Engine {

        public static void Process<R>() {
            try {
                CollectTranslations<R>();

                //Translatable.AssignLanguages<R>();

                //Term.SetTermNames<R>();

                CheckForInstanceFields<R>();
            }
            catch (Exception e){
                Log.Error.OnException(GetDeepestException(e));
            }
        }

        static void CollectTranslations<R>() {
            var methods = typeof(R).GetNestedTypes().Select(x => x.GetMethod("CollectTranslations", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)).Where(x => x != null);
            foreach (var m in methods) {
                m.Invoke(null, new object[0]);
            }
        }

        /*static void AssignUrls<R>() {
            var fields = typeof(R).GetNestedTypes().SelectMany(x => x.GetFields());
            foreach (var f in fields) {
                if (f is IPage) { 
                    
                }
            }
        }*/

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

        public static void Generate<R>() {
            Directory.Delete(Context.Current.OutputRootDirectory, true);            

            var fields = typeof(R).GetNestedTypes().SelectMany(x => x.GetFields(BindingFlags.Static | BindingFlags.Public));
            foreach (var f in fields) {
                var value = f.GetValue(null);
                (value as IPage)?.Create();
            }

        }


    }
}