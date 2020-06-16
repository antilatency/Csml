using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public class Container : Container<Container> {
        public Container(string tag, params string[] classes):base(tag, classes) {}
    }

    public class Container<T> : Collection<T> where T : Container<T> {
        public string Tag { get; set; }
        public List<string> Classes;
        public Container(string tag, params string[] classes) {
            Tag = tag;
            Classes = classes.ToList();
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode($"<{Tag}>").Do(x => {
                foreach (var c in Classes) x.AddClass(c);
                x.Add(base.Generate(context));
            });
        }
    }
}