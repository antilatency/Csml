using Csml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csml {
    public class ScopeUtils {

        public static BindingFlags PropertyBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static IEnumerable<Type> AllStatic {
            get {
                return Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(Scope)))
                    .Where(x => !x.IsGenericType);
            }
        }


        public static IEnumerable<T> GetScopePropertiesOfType<ScopeType,T>() {
            var result = typeof(ScopeType).GetProperties(PropertyBindingFlags)
                .Where(x=>typeof(T).IsAssignableFrom(x.PropertyType))                
                .Select(x => (T)x.GetValue(null));
            return result;
        }


        public static IEnumerable<Scope> All {
            get {
                return AllStatic.Select(x => (Scope)Activator.CreateInstance(x));
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

            foreach (var p in t.GetProperties(PropertyBindingFlags)) {
                GetOnce.WrapPropertyGetter(p);
            }
        }



    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    public partial class Scope {

        protected Scope() { }

        

        private IEnumerable<IMaterial> GetMaterials() {
            //var pros = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            //var o = pros.Where(x => x.PropertyType.ImplementsInterface(typeof(IPage)));

            return GetType().GetProperties(ScopeUtils.PropertyBindingFlags)
            .Where(x => x.PropertyType.ImplementsInterface(typeof(IMaterial))).Select(x => x.GetValue(this) as IMaterial);

        }

        public void Verify() {
            GetMaterials().ToList();
        }

        public ITemplate GetTemplate() {
            var type = GetType();
            while (type != null) {
                var template = type
                .GetProperty(nameof(Template), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(null) as ITemplate;
                if (template != null) return template;

                type = type.DeclaringType ?? typeof(Scope);
            }
            return null;
        }

        public void Generate(Context context) {
            var template = GetTemplate();

            var materials = GetMaterials();
            var languages = Language.All;
            var matrix = new Dictionary<string, Dictionary<Language, IMaterial>>();
            foreach (var m in materials) {
                if (!matrix.ContainsKey(m.NameWithoutLanguage)) {
                    matrix.Add(m.NameWithoutLanguage, new Dictionary<Language, IMaterial>());
                }
                var n = matrix[m.NameWithoutLanguage];
                if (m.Language == null) {
                    foreach (var l in languages) {
                        n.Add(l, m);
                    }
                } else {
                    n.Add(m.Language, m);
                }
            }

            foreach (var n in matrix) {
                Application.SiteMap.Add(n.Value.First().Value);
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
                //Log.Info.Here($"Generation Page {n.Key}");
                foreach (var l in n.Value) {
                    //Log.Info.Here($"Generation Page {n.Key} context {l.Key}");
                    context.Language = l.Key;

                    template.Generate(context, l.Value);
                    //l.Value.Generate(context);
                }
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
}