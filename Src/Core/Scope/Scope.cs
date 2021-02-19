using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csml {

    public class BaseScope {
        protected static IPageTemplate Template => new BlankTemplate();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    public partial class Scope: BaseScope {

        protected Scope() { }
        
        private IEnumerable<System.Reflection.PropertyInfo> GetMaterialsAsTypes() {
            return GetType().GetProperties(ScopeHelper.PropertyBindingFlags)
                .Where(x => x.PropertyType.ImplementsInterface(typeof(IMaterial)));
        }

        public IPageTemplate GetTemplate() {
            return GetType().GetProperty("Template", ScopeHelper.PropertyBindingFlags)?.GetValue(null, null) as IPageTemplate;
        }

        public Dictionary<string, Dictionary<Language, PropertyInfo>> GenerateMaterialTypesMatrix(Context context) {
            var materials = GetMaterialsAsTypes();
            var languages = Language.All;

            var matrix = new Dictionary<string, Dictionary<Language, PropertyInfo>>();

            foreach (var material in materials) {
                //Console.WriteLine($"material {material.Title}");
                var materialName = Element.GetNameWithoutLanguage(material);

                if (!matrix.ContainsKey(materialName)) {
                    matrix.Add(materialName, new Dictionary<Language, PropertyInfo>());
                }

                var translations = matrix[materialName];

                if (Element.GetLanguage(material) == null) {
                    foreach (var lang in languages) {
                        translations.Add(lang, material);
                    }
                } else {
                    translations.Add(Element.GetLanguage(material), material);
                }
            }

            foreach (var n in matrix) {
                var translations = n.Value;

                if (translations.Count != languages.Length) {
                    var replacementPage = translations[languages.First(x => translations.ContainsKey(x))];
                    foreach (var l in languages) {
                        if (!translations.ContainsKey(l))
                            translations.Add(l, replacementPage);
                    }
                }
            }
            return matrix;
        }

        public void Generate(Context context) {
            var materialMatrix = GenerateMaterialTypesMatrix(context);
            var template = GetTemplate();

            foreach (var n in materialMatrix) {
                var translations = n.Value;

                CsmlApplication.SiteMapMaterials.Add(translations.First().Value.GetValue(this) as IMaterial);

                foreach (var l in translations) {
                    context.Language = l.Key;
                    template.Generate(context, l.Value.GetValue(this) as IMaterial);
                }
            }
        }
    }
}