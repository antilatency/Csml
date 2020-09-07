using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Csml.CsmlPredefined.Diagnostics_Assets;


namespace Csml {
    public partial class CsmlPredefined : Scope {
        public partial class Diagnostics_Assets : Scope {
            public static IEnumerable<Type> GetTopLevelScopes() {
                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => !x.IsNested)
                    .Where(x => x.IsSubclassOf(typeof(Scope)))
                    .Where(x => !x.IsGenericType);
                return types;
            }


            public static IEnumerable<LanguageSelector<IMaterial>> GetMaterialsFromScope(Type scope) {
                return scope.GetProperties(ScopeHelper.PropertyBindingFlags)
                        .Where(x => typeof(LanguageSelector<IMaterial>).IsAssignableFrom(x.PropertyType))
                        .Select(x => x.GetValue(null) as LanguageSelector<IMaterial>);
            }

            public static IEnumerable<IElement> GetAllPages() {
                var scopeTypes = ScopeHelper.AllStatic;

                foreach (var scope in scopeTypes) {
                    var materials = GetMaterialsFromScope(scope);

                    if (materials.Count() > 0) {
                        yield return new Spoiler(scope.FullName)[new UnorderedList()[materials]];
                    }
 
                }

                yield break;
            }

            public static bool CheckTranslationsTopology(LanguageSelector<IMaterial> material) {
                return true;
            }

            public static IEnumerable<IElement> GetPagesIssues() { 
                var scopeTypes = ScopeHelper.AllStatic;
                var emptyMaterials = new List<LanguageSelector<IMaterial>>();
                var missingTranslations = new Dictionary<Language, List<LanguageSelector<IMaterial>>>();

                foreach (var scope in scopeTypes) {
                    var materials = GetMaterialsFromScope(scope);

                    foreach (var material in materials) {
                        if (!material.HasTarget) {
                            emptyMaterials.Add(material);
                        } else {
                            foreach (var language in Language.All) {
                                if (!material.HasTranslation(language)) {
                                    List<LanguageSelector<IMaterial>> missing = null;

                                    if (!missingTranslations.TryGetValue(language, out missing)) {
                                        missing = new List<LanguageSelector<IMaterial>>();
                                        missingTranslations[language] = missing;
                                    }

                                    missing.Add(material);
                                }
                            }
                        }
                    }
                }

                if (emptyMaterials.Count > 0) {
                    yield return new Spoiler("Empty materials")[new UnorderedList()[emptyMaterials]];
                }

                foreach (var kv in missingTranslations) {
                    var language = kv.Key;
                    var materials = kv.Value;

                    if (materials != null && materials.Count > 0) {
                        yield return new Spoiler($"Missing \"{language.Name}\" translation")[new UnorderedList()[materials]];
                    }
                }

                yield break;
            }
        }

        public static Material Diagnostics => new Material("Diagnostics", null, $"This page provide diagnostics information about website. This page is auto generated.")
            [new Section("Issues")
                [new UnorderedList()
                    [GetPagesIssues()]
                ]
            ]
            [new Section("Full list of pages")
                [new UnorderedList()
                    [GetAllPages()]
                ]   
            ];
    }
}