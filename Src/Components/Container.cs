using Htmlilka;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public class Container : Container<Container> {
        public Container(string tag, params string[] classes):base(tag, classes) {}
    }

    public class Container<T> : Collection<T> where T : Container<T> {
        public string Tag { get; set; }
        public string[] Classes;
        public Container(string tag, params string[] classes) {
            Tag = tag;
            Classes = classes;
        }

        private Dictionary<object, Node> previouslyGenerated = new Dictionary<object, Node>();

        public override Node Generate(Context context) {
            return new Tag(Tag).AddClasses(Classes).Add(base.Generate(context));
        }
    }
}