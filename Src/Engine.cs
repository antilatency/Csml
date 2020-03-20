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

        public static void Generate<R>(Context context, bool clean = true) {
            if (clean) {
                if (Directory.Exists(context.OutputRootDirectory))
                    Directory.Delete(context.OutputRootDirectory, true);
            }

            foreach (var i in context.AssetsToCopy) {
                var dest = Path.Combine(context.OutputRootDirectory, Context.GetContentRelativePath(i, context.SourceRootDirectory));
                Utils.CreateDirectories(Path.GetDirectoryName(dest));
                if (!File.Exists(dest) | context.ForceRebuildAssets)
                    File.Copy(i, dest, true);
            }

            var fields = typeof(R).GetNestedTypes().SelectMany(x => x.GetFields(BindingFlags.Static | BindingFlags.Public));
            foreach (var f in fields) {
                var value = f.GetValue(null);
                (value as IPage)?.Create(context);
            }

        }


    }
}