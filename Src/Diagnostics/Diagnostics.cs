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
                var scopeTypes = ScopeUtils.AllStatic;// GetTopLevelScopes();


                foreach (var t in scopeTypes) {
                    var multiLanguageGroups = 
                        t.GetProperties(ScopeUtils.PropertyBindingFlags)
                        .Where(x => x.PropertyType == typeof(MultiLanguageGroup))
                        .Select(x=>x.GetValue(null) as MultiLanguageGroup);
                    if (multiLanguageGroups.Count() == 0) continue;
                    
                    var listOfMultiLanguageGroups = new UnorderedList()[multiLanguageGroups];
                    
                    var element = new Paragraph($"{t.FullName} materials: {listOfMultiLanguageGroups}");


                    yield return element;
                }
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