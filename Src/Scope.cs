using Csml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csml {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    public class Scope {
        protected Scope() { }

        public static IEnumerable<Type> AllStatic {
            get {
                return Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(Scope)))
                    .Where(x => !x.IsGenericType);
            }
        }

        public static IEnumerable<Scope> All {
            get {
                return AllStatic.Select(x => (Scope)Activator.CreateInstance(x));
            }
        }

        private IEnumerable<IPage> GetPages() {
            //var pros = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            //var o = pros.Where(x => x.PropertyType.ImplementsInterface(typeof(IPage)));

            return GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(x => x.PropertyType.ImplementsInterface(typeof(IPage))).Select(x => {
                return x.GetValue(this) as IPage;
            }
                );

        }

        public void Verify() {
            GetPages().ToList();
        }

        public void Generate(Context context) {




            var pages = GetPages();

            var languages = Language.All;

            var matrix = new Dictionary<string, Dictionary<Language, IPage>>();

            foreach (var p in pages) {
                if (!matrix.ContainsKey(p.NameWithoutLanguage)) {
                    matrix.Add(p.NameWithoutLanguage, new Dictionary<Language, IPage>());
                }
                var n = matrix[p.NameWithoutLanguage];
                if (p.Language == null) {
                    foreach (var l in languages) {
                        n.Add(l, p);
                    }
                } else {
                    n.Add(p.Language, p);
                }
            }

            foreach (var n in matrix) {
                if (n.Value.Count != languages.Count) {
                    var replacementPage = n.Value[languages.First(x => n.Value.ContainsKey(x))];
                    foreach (var l in languages) {
                        if (!n.Value.ContainsKey(l))
                            n.Value.Add(l, replacementPage);
                    }
                }
            }

            foreach (var n in matrix) {
                Log.Info.Here($"Generation Page {n.Key}");
                foreach (var l in n.Value) {
                    Log.Info.Here($"Generation Page {n.Key} context {l.Key}");
                    context.Language = l.Key;
                    l.Value.Create(context);
                }
            }

        }

        public static void EnableGetOnce() {
            var allStatic = AllStatic;
            foreach (var i in allStatic) {
                /*var thisType = i.GetProperty("ThisType", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
                if (i != thisType) { 
                    Log.Error.Unknown($"Type {i.FullName} is invalid: Scope types must pass final type to generic parameter of parent type.")
                }*/
            }
            AllStatic.ForEach(x =>
                EnableGetOnce(x)
                //x.GetMethod("EnableGetOnce", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Invoke(null, new object[0])


                );
        }
        public static void EnableGetOnce(Type t) {
            var nonStaticMembers = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (nonStaticMembers.Count() > 0) {
                Log.Error.Here($"Scope {t.Name} contains non static prooperty {nonStaticMembers.First().Name}");
            }

            foreach (var p in t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                GetOnce.WrapPropertyGetter(p);
            }
        }

        public static Type ThisType{
            get {
                StackTrace stackTrace = new StackTrace(true);
                var result = stackTrace.GetFrame(1).GetMethod().DeclaringType;
                return result;
            }
        }
    }


    
    //public class Scope<T> : Scope where T: Scope<T>, new(){
    //}



}