using Htmlilka;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public class ContainerWithSpecificChildren<T> : Collection<T> where T : ContainerWithSpecificChildren<T> {
        public string Tag { get; set; }
        public string[] Classes;
        public string ChildTag { get; set; }

        public ContainerWithSpecificChildren(string tag, string childTag, params string[] classes) {
            Tag = tag;
            ChildTag = childTag;
            Classes = classes;
        }

        public override Node Generate(Context context) {
            var result = new Tag(Tag).AddClasses(Classes);


            /*foreach (var i in Elements) {
                result.AddTag(ChildTag, x => {
                    x.Add(i.Generate(context));
                });
            }*/


            Generate(result, Elements, context);


            return result;
        }

        private void Generate(Tag parent, IEnumerable<IElement> elements, Context context) {
            foreach (var i in elements) {
                if (i is LazyCollection collection) {
                    Generate(parent, collection.Elements, context);
                } else {
                    parent.AddTag(ChildTag, x => {
                        x.Add(i.Generate(context));
                    });
                }
                
            }
        }
    }
}