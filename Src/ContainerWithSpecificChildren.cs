using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public class ContainerWithSpecificChildren<T> : Container<T> where T : ContainerWithSpecificChildren<T> {
        public string ChildTag { get; set; }
        public ContainerWithSpecificChildren(string tag, string childTag, params string[] classes) : base(tag, classes) {
            ChildTag = childTag;
        }
        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode($"<{Tag}>").Do(x => {
                foreach (var c in Classes) x.AddClass(c);
                foreach (var e in Elements) {
                    x.Add($"<{ChildTag}>").Add(e.Generate(context));
                }
            });
        }
    }
}