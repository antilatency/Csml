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
            foreach (var i in Elements) {
                result.AddTag(ChildTag, x => {
                    x.Add(i.Generate(context));
                });
            }
            return result;
        }
    }
}