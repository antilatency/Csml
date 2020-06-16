using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public class ContainerWithSpecificChildren<T> : Collection<T> where T : ContainerWithSpecificChildren<T> {
        public string Tag { get; set; }
        public List<string> Classes;
        public string ChildTag { get; set; }

        public ContainerWithSpecificChildren(string tag, string childTag, params string[] classes) {
            Tag = tag;
            ChildTag = childTag;
            Classes = classes.ToList();
        }
        
        /*public ContainerWithSpecificChildren(string tag, params string[] classes) : base(tag, classes) {
            ChildTag = childTag;
        }*/
        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode($"<{Tag}>").Do(x => {
                foreach (var c in Classes) x.AddClass(c);
                /*foreach (var e in Elements) {
                    x.Add($"<{ChildTag}>").Add(e.Generate(context));
                }*/
                foreach (var t in base.Generate(context)) {
                    x.Add($"<{ChildTag}>").AppendChild(t);
                }
            });
        }
    }
}