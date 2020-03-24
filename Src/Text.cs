using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            Func<string,string[]> lineSplit = x=> x.Replace("\r", "").Trim('\n').Split('\n');

            var args = Elements.ToArray();
            //var args = Elements.Select(x => x.Generate(context).ToHtml()).ToArray();
            Regex regex = new Regex(@"{(\d+):?(.*?)}");
            var matches = regex.Matches(Format);
            var pose = 0;
            
            for (int i = 0; i < matches.Count; i++) {
                if (pose < matches[i].Index) {
                    var text = Format.Substring(pose, matches[i].Index - pose);
                    var lines = lineSplit(text);
                    if (!string.IsNullOrEmpty(lines[0]))
                        yield return HtmlNode.CreateNode(lines[0]);
                    for (int l = 1; l < lines.Length; l++) {
                        yield return HtmlNode.CreateNode("<br>");
                        if (!string.IsNullOrEmpty(lines[l]))
                            yield return HtmlNode.CreateNode(lines[l]);
                    }
                }

                foreach (var eg in args[i].Generate(context))
                    yield return eg;

                pose = matches[i].Index + matches[i].Length;
            }

            if (pose < Format.Length) {
                var text = Format.Substring(pose, Format.Length - pose);
                var lines = lineSplit(text);
                yield return HtmlNode.CreateNode(lines[0]);
                for (int l = 1; l < lines.Length; l++) {
                    yield return HtmlNode.CreateNode("<br>");
                    if (!string.IsNullOrEmpty(lines[l]))
                        yield return HtmlNode.CreateNode(lines[l]);
                }
            }
        }

    }
}