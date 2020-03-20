using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public sealed class Text: Text<Text> {

        private Text() { }
        public Text(string s) {
            Format = s;
        }

        public Text(FormattableString formattableString) {
            Format = formattableString.Format;
            var arguments = formattableString.GetArguments();
            var filteredArguments = new List<IElement>();
            int filteredArgumentID = -1;
            for (int i = 0; i < arguments.Length; i++) {
                if (arguments[i] is IElement) {
                    Add(arguments[i] as IElement);
                    filteredArgumentID++;
                    if (i != filteredArgumentID) {
                        Format = Format.Replace("{" + i + "}", "{" + filteredArgumentID + "}");
                    }
                } else {
                    Format = Format.Replace("{" + i + "}", arguments[i].ToString());
                }
            }

        }
        public static implicit operator Text(FormattableString x) => new Text(x);

    }

    public class Text<T> : Collection<T> where T : Text<T> {
        //private Func<object> 
        public string Format;

        


        public override IEnumerable<HtmlNode> Generate(Context context) {
            var args = Elements.Select(x => x.Generate(context).ToHtml()).ToArray();

            var formatWithBr = Format.Replace("\r", "").Replace("\n","<br>");


            var content = string.Format(formatWithBr, args);
   
            return new HtmlNode[] { HtmlNode.CreateNode($"<div>{content}</div>") };
        }

    }
}