using System;
using System.Collections.Generic;
using Htmlilka;

namespace Csml {

    public sealed class Collection : Collection<Collection> {
        public Collection(bool wrapStringToParagraph = true): base(wrapStringToParagraph) {}
    }

    public class Collection<T>: LazyCollection<T> where T : Collection<T> {
        private List<IElement> List = new List<IElement>();
        public bool WrapStringToParagraph;
        public Collection(bool wrapStringToParagraph = true) {
            WrapStringToParagraph = wrapStringToParagraph;
            Elements = List;
        }

        public virtual T Add(IElement element) {
            List.Add(element);
            return this as T;
        }
        public virtual T Add(FormattableString formattableString) {
            if (WrapStringToParagraph)
                List.Add(new Paragraph(formattableString));
            else
                List.Add(new Text(formattableString));
            return this as T;
        }

        public virtual T Add(List<FormattableString> formattableString) {
            formattableString.ForEach(x => {
                if (WrapStringToParagraph)
                    List.Add(new Paragraph(x));
                else
                    List.Add(new Text(x));
            });

            return this as T;
        }


        /*public static object SimplifyFormattableString(FormattableString formattableString) {
            if (formattableString.Format == "{0}") return formattableString.GetArgument(0);
            if (formattableString.ArgumentCount == 0) return formattableString.Format;
            return formattableString;
        }*/

        public T this[FormattableString element] { get => Add(element); }
        public T this[List<FormattableString> element] { get => Add(element); }
        public T this[IElement element] { get => Add(element); }

        public T this[IEnumerable<IElement> element] { get => Add(new LazyCollection(element)); }

        //public T this[Func<IEnumerable<IElement>> element] { get => Add(new LazyCollection(element())); }
    }


    public sealed class LazyCollection: LazyCollection<LazyCollection> {
        public LazyCollection(IEnumerable<IElement> elements) {
            Elements = elements;
        }
    }

    public class LazyCollection<T> : Element<T> where T : LazyCollection<T> {
        public IEnumerable<IElement> Elements { get; protected set; }
        
        public override Node Generate(Context context) {
            var result = new Tag(null);
            foreach (var e in Elements)
                result.Add(e.Generate(context));
            return result;
        }
    }

}