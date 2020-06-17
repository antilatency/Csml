using Csml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Csml {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    public partial class Scope {

        protected Scope() { }

        private IEnumerable<IMaterial> GetMaterials() {
            //var pros = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            //var o = pros.Where(x => x.PropertyType.ImplementsInterface(typeof(IPage)));

            return GetType().GetProperties(ScopeHelper.PropertyBindingFlags)
            .Where(x => x.PropertyType.ImplementsInterface(typeof(IMaterial))).Select(x => x.GetValue(this) as IMaterial);

        }

        public void Verify() {
            GetMaterials().ToList();
        }

        public IPageTemplate GetTemplate() {
            var type = GetType();
            while (type != null) {
                var template = type
                .GetProperty(nameof(Template), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(null) as IPageTemplate;
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
                CsmlWorkspace.Current.SiteMapMaterials.Add(n.Value.First().Value);
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