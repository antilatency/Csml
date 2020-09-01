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


            public static IEnumerable<IElement> GetAllPages() {
                var scopeTypes = ScopeHelper.AllStatic;

                foreach (var scope in scopeTypes) {
                    var materials = scope.GetProperties(ScopeHelper.PropertyBindingFlags)
                        .Where(x => typeof(LanguageSelector<IMaterial>).IsAssignableFrom(x.PropertyType))
                        .Select(x => x.GetValue(null) as LanguageSelector<IMaterial>);

                    if (materials.Count() > 0) {
                        var materialsList = new UnorderedList();
                        materials.ForEach(x => materialsList.Add(x));

                        yield return new Paragraph($"{scope.FullName} materials: {materialsList}");
                    }
 
                }

                yield break;
            }
        }

        public static Material Diagnostics => new Material("Diagnostics", null, $"This page provide diagnostics information about website. This page is auto generated.")
            [new Section("Full list of pages")
                [new OrderedList()
                    [GetAllPages()]
                
                ]
            ];
    }
}