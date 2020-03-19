using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace Csml {

    public class Collection<T>: Element<T> where T : Collection<T> {
        public List<IElement> elements = new List<IElement>();

        public T Add(IElement element) {
            elements.Add(element);
            return this as T;
        }
        public T Add(FormattableString formattableString) {
            elements.Add( new Text(formattableString));
            return this as T;
        }


        public static object SimplifyFormattableString(FormattableString formattableString) {
            if (formattableString.Format == "{0}") return formattableString.GetArgument(0);
            if (formattableString.ArgumentCount == 0) return formattableString.Format;
            return formattableString;
        }

        public T this[FormattableString element] { get => Add(element); }
        public T this[IElement element] { get => Add(element); }

        public T this[Action<T> lambda] {
            get {
                lambda(this as T);
                return this as T;
            }
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            return elements.SelectMany(x => x.Generate(context));
        }


    }
}