using System.Collections.Generic;
using System.Linq;

namespace Csml {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    public partial class Scope {

        protected Scope() { }

        private IEnumerable<IMaterial> GetMaterials() {
            return GetType().GetProperties(ScopeHelper.PropertyBindingFlags)
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IMaterial)))
                .Select(x => x.GetValue(this) as IMaterial);
        }

        public IPageTemplate GetTemplate() {
            return GetType().GetProperty(nameof(Template), ScopeHelper.PropertyBindingFlags)?.GetValue(null, null) as IPageTemplate;
        }

        public void Generate(Context context) {
            var template = GetTemplate();
            var materials = GetMaterials();
            var languages = Language.All;

            var matrix = new Dictionary<string, Dictionary<Language, IMaterial>>();

            foreach (var material in materials) {
                var materialName = material.NameWithoutLanguage;

                if (!matrix.ContainsKey(materialName)) {
                    matrix.Add(materialName, new Dictionary<Language, IMaterial>());
                }

                var translations = matrix[materialName];

                if (material.Language == null) {
                    foreach (var lang in languages) {
                        translations.Add(lang, material);
                    }
                } else {
                    translations.Add(material.Language, material);
                }
            }

            foreach (var n in matrix) {
                var translations = n.Value;

                CsmlWorkspace.Current.SiteMapMaterials.Add(translations.First().Value);

                if (translations.Count != languages.Count) {
                    var replacementPage = translations[languages.First(x => translations.ContainsKey(x))];
                    foreach (var l in languages) {
                        if (!translations.ContainsKey(l))
                            translations.Add(l, replacementPage);
                    }
                }

                foreach (var l in translations) {
                    context.Language = l.Key;
                    template.Generate(context, l.Value);
                }
            }
        }
    }
}