using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace Csml {

    

    public sealed class Collection : Collection<Collection> {
    }

    public class Collection<T>: LazyCollection<T> where T : Collection<T> {
        private List<IElement> List = new List<IElement>();

        public Collection(){
            Elements = List;
        }

        public T Add(IElement element) {
            List.Add(element);
            return this as T;
        }
        public T Add(FormattableString formattableString) {
            List.Add( new Paragraph(formattableString));
            return this as T;
        }


        public static object SimplifyFormattableString(FormattableString formattableString) {
            if (formattableString.Format == "{0}") return formattableString.GetArgument(0);
            if (formattableString.ArgumentCount == 0) return formattableString.Format;
            return formattableString;
        }

        public T this[FormattableString element] { get => Add(element); }
        public T this[IElement element] { get => Add(element); }

        public T this[IEnumerable<IElement> element] { get => Add(new LazyCollection(element)); }

        public T this[Func<IEnumerable<IElement>> element] { get => Add(new LazyCollection(element())); }
    }


    public sealed class LazyCollection: LazyCollection<LazyCollection> {
        public LazyCollection(IEnumerable<IElement> elements) {
            Elements = elements;
        }
    }

    public class LazyCollection<T> : Element<T> where T : LazyCollection<T> {
        public IEnumerable<IElement> Elements { get; protected set; }
        
        public override IEnumerable<HtmlNode> Generate(Context context) {
            return Elements.SelectMany(x => x.Generate(context));
        }
    }

}