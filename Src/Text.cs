using System;
using System.Linq;
using HtmlAgilityPack;

namespace Csml {
    public sealed class Text: Text<Text> {
        public Text(FormattableString formattableString) {
            Format = formattableString.Format;
            foreach (var a in formattableString.GetArguments()) {
                Add(a);
            }
        }
        public static implicit operator Text(FormattableString x) => new Text(x);

    }

    public class Text<T> : Collection<T> where T : Text<T> {
        //private Func<object> 
        public string Format;

        public override HtmlNode Generate() {
            var args = elements.Where(x => x is IElement).Select(x => (x as IElement)).Select(x => x.Generate().OuterHtml).ToArray();
            var content = string.Format(Format, args);
   
            return HtmlNode.CreateNode($"<div>{content}</div>");
        }

    }
}