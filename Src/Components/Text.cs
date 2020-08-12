using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Htmlilka;

namespace Csml {
    public sealed class Text: Text<Text> {

        //public Text(string s) :base(s) {}
        public Text(FormattableString formattableString) : base(formattableString) { }


        //public static implicit operator Text(FormattableString x) => new Text(x);
    }

    public sealed class Paragraph : Text<Paragraph> {
        //public Paragraph(string s) : base(s) { }
        public Paragraph(FormattableString formattableString) : base(formattableString) { }

        public override Node Generate(Context context) {
            return new Tag("div")
                .Add(base.Generate(context))
                .AddClasses("Text");
        }


    }



    public class Text<T> : Collection<T> where T : Text<T> {
        //private Func<object> 
        public string Format;

        protected Text() { }

        /*public Text(string s) {
            Format = s;
        }*/

        public Text(FormattableString formattableString) {
            Format = formattableString.Format;
            var arguments = formattableString.GetArguments();
            var filteredArguments = new List<IElement>();
            int filteredArgumentID = -1;
            for (int i = 0; i < arguments.Length; i++) {
                if (arguments[i] is IElement) {
                    Add(arguments[i] as IElement);
                    filteredArgumentID++;
                    if (i != filteredArgumentID) {                                                 //fix i
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
            public Tag Result = new Tag(null);
            public Stack<Tag> currentElements = new Stack<Tag>();

            public void Add(Node node) {
                if (currentElements.Count == 0) {
                    Result.Add(node);
                } else {
                    currentElements.Peek().Add(node);
                }
            }
            
            public void AddText(string text) {
                if (string.IsNullOrEmpty(text)) return;
                var node = new TextNode(text);
                Add(node);
            }
            public void Br() {
                var node = new VoidTag("br");
                Add(node);
            }

            public bool Tag(string name) {
                var close = (currentElements.Count !=0) && (currentElements.Peek().Name == name);

                if (close) {
                    Close();
                    return false;
                }
                var node = new Tag(name);
                Add(node);
                /*if (currentElement == null) {
                    Result.Add(node);
                } else {
                    currentElement.Add(node);                    
                }*/
                currentElements.Push(node);
                return true;
            }
            private void Close() {
                currentElements.Pop();
                //if (currentElement.Name == "#document") currentElement = null;
            }
        }

        public void Markdown(string text, MarkdownState markdownState) {
            int start = 0;
            int length = 0;

            Action subText =()=>{
                markdownState.AddText(text.Substring(start, length));
                start += length;
                length = 0;
            };

            text = text.Replace((char)1, '{').Replace((char)2, '}');

            for (int i = 0; i < text.Length; i++) {
                Char c = text[i];
                if (c == '\r') continue;
                if (c == '\n') {
                    subText();
                    markdownState.Br();
                    start = i + 1;
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

        //private Dictionary<object, Node> generated = new Dictionary<object, Node>();

        //public static int Max;

        public override Node Generate(Context context) {

            /*var uniqness = Tuple.Create(context.AForbidden, context.FormatString, context.Language);
            Node backup;
            if (generated.TryGetValue(uniqness, out backup)) {
                return backup;
            }

            Max++;*/

            MarkdownState markdownState = new MarkdownState();

            var args = Elements.ToArray();

            Regex regexB = new Regex("}}(?!})");
            Format = Format.Replace("{{", "\u0001");
            Format = regexB.Replace(Format, "\u0002");

            Regex regex = new Regex(@"{(\d+):?(.*?)}");
            var matches = regex.Matches(Format);
            var pose = 0;
            var numMatches = matches.Count;
            for (int i = 0; i < numMatches+1; i++) {
                var endOfRegion = i == numMatches ? Format.Length : matches[i].Index;
                if (pose < endOfRegion) {
                    var text = Format.Substring(pose, endOfRegion - pose);
                    Markdown(text, markdownState);
                }
                if (i != numMatches) {
                    if (!string.IsNullOrEmpty(matches[i].Groups[2].Value)) {
                        context.FormatString = matches[i].Groups[2].Value;
                    } else {
                        context.FormatString = null;
                    }
                    markdownState.Add(args[i].Generate(context));
                    pose = matches[i].Index + matches[i].Length;
                }
            }

            //generated[uniqness] = markdownState.Result;

            return markdownState.Result;
        }

    }
}