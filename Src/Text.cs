using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Csml {
    public sealed class Text: Text<Text> {

        public Text(string s) :base(s) {}
        public Text(FormattableString formattableString) : base(formattableString) { }


        //public static implicit operator Text(FormattableString x) => new Text(x);
    }

    public sealed class Paragraph : Text<Paragraph> {
        public Paragraph(string s) : base(s) { }
        public Paragraph(FormattableString formattableString) : base(formattableString) { }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode("<div>").Do(x=> {
                x.Add(base.Generate(context));
                x.AddClass("text");
            });
        }


    }



    public class Text<T> : Collection<T> where T : Text<T> {
        //private Func<object> 
        public string Format;

        protected Text() { }

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
                    var text = "";
                    if (arguments[i] is Func<string>) {
                        text = (arguments[i] as Func<string>)();
                    } else {
                        text = arguments[i].ToString();
                    }
                    Format = Format.Replace("{" + i + "}", text);
                }
            }

        }

        public override string ToString() {
            return string.Format(Format, Elements.Select(x=>x.ToString()).ToArray());
        }

        public class MarkdownState {
            public bool Code;
            public List<HtmlNode> Result = new List<HtmlNode>();
            public HtmlNode currentElement;
            public void Add(HtmlNode node) {
                if (currentElement == null) {
                    Result.Add(node);
                } else {
                    currentElement.AppendChild(node);
                }
            }
            
            public void AddText(string text) {
                if (string.IsNullOrEmpty(text)) return;
                var node = HtmlNode.CreateNode(HtmlDocument.HtmlEncode(text));

                Add(node);
            }
            public void Br() {
                var node = HtmlNode.CreateNode("<br>");
                Add(node);
            }

            public bool Tag(string name) {
                var close = (currentElement != null) && (currentElement.Name == name);

                if (close) {
                    Close();
                    return false;
                }
                var node = HtmlNode.CreateNode($"<{name}>");
                if (currentElement == null) {
                    Result.Add(node);
                } else {
                    currentElement.AppendChild(node);
                    
                }
                currentElement = node;
                return true;
            }
            private void Close() {
                currentElement = currentElement.ParentNode;
                if (currentElement.Name == "#document") currentElement = null;
            }
        }

        public void Markdown(string text, MarkdownState markdownState) {
            //StringBuilder stringBuilder = new StringBuilder();
            int start = 0;
            int length = 0;

            //List<HtmlNode> result = new List<HtmlNode>();

            Action subText =()=>{
                markdownState.AddText(text.Substring(start, length));
                start += length;
                length = 0;
            };

            /*Func<string, bool> Is = x => {
                if (markdownState.currentElement == null) return false;
                return markdownState.currentElement.Name == x;
            };*/


            for(int i=0; i < text.Length; i++ ) {
                Char c = text[i];
                if (c == '\r') continue;
                if (c == '\n') {
                    subText();
                    markdownState.Br();
                    start = i+1;
                    continue;
                };
                if (c == '`') {
                    subText();
                    markdownState.Code = markdownState.Tag("code");
                    start = i + 1;
                    continue;
                };
                if (!markdownState.Code) { 
                    if (c == '*') {
                        subText();
                        markdownState.Tag("b");
                        start = i + 1;
                        continue;
                    };
                    if (c == '_') {
                        subText();
                        markdownState.Tag("i");
                        start = i + 1;
                        continue;
                    };
                    if (c == '~') {
                        subText();
                        markdownState.Tag("s");
                        start = i + 1;
                        continue;
                    };
                }
                length++;
            }
            subText();
            //return markdownState.Result;

        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            MarkdownState markdownState = new MarkdownState();
            Func<string,string[]> lineSplit = x=> x.Replace("\r", "").Split('\n');

            var args = Elements.ToArray();
            //var args = Elements.Select(x => x.Generate(context).ToHtml()).ToArray();
            Regex regex = new Regex(@"{(\d+):?(.*?)}");
            var matches = regex.Matches(Format);
            var pose = 0;
            var numMatches = matches.Count;
            for (int i = 0; i < numMatches+1; i++) {
                var endOfRegion = i == numMatches ? Format.Length : matches[i].Index;
                if (pose < endOfRegion) {
                    var text = Format.Substring(pose, endOfRegion - pose);
                    Markdown(text, markdownState);

                    /*var lines = lineSplit(text);
                    if (!string.IsNullOrEmpty(lines[0]))
                        yield return HtmlNode.CreateNode(lines[0]);
                    for (int l = 1; l < lines.Length; l++) {
                        yield return HtmlNode.CreateNode("<br>");
                        if (!string.IsNullOrEmpty(lines[l]))
                            yield return HtmlNode.CreateNode(lines[l]);
                    }*/
                }
                if (i != numMatches) {
                    foreach (var eg in args[i].Generate(context))
                        markdownState.Add(eg);
                    pose = matches[i].Index + matches[i].Length;
                }
            }

            return markdownState.Result;

            /*if (pose < Format.Length) {
                var text = Format.Substring(pose, Format.Length - pose);
                var lines = lineSplit(text);
                if (!string.IsNullOrEmpty(lines[0]))
                    yield return HtmlNode.CreateNode(lines[0]);
                for (int l = 1; l < lines.Length; l++) {
                    yield return HtmlNode.CreateNode("<br>");
                    if (!string.IsNullOrEmpty(lines[l]))
                        yield return HtmlNode.CreateNode(lines[l]);
                }
            }*/
        }

    }
}