using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Csml {
    public abstract class Code<T> : Element<T> where T : Code<T> {
        protected object Source;
        protected Code() {
        }

        protected Code(T other) : this() {
            Source = other.Source;
        }

        protected abstract string GetFinalSourceCode();
        protected abstract Range? GetLineSpan();
        protected abstract ColorCode.ILanguage ProgrammingLanguage { get; } 


        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (Source is GithubFile) {
                var lineSpan = GetLineSpan();
                yield return HtmlNode.CreateNode("<a>").Do(x => {
                    x.AddClass("github-link");
                    x.SetAttributeValue("target", "_blank");
                    var href = (Source as GithubFile).content.Result[0].HtmlUrl;
                    
                    if (lineSpan.HasValue) {
                        x.SetAttributeValue("href", $"{href}#L{lineSpan.Value.Start.Value + 1}-L{lineSpan.Value.End.Value + 1}");
                    } else {
                        x.SetAttributeValue("href", $"{href}");
                    }
                });


            }

            var code = GetFinalSourceCode();

            //https://github.com/WilliamABradley/ColorCode-Universal

            var formatter = new ColorCode.HtmlFormatter();

            var html = formatter.GetHtmlString(code, ProgrammingLanguage);
            yield return HtmlNode.CreateNode(html).Do(x => {
                x.Attributes.RemoveAll();
                x.AddClass("code");
            });
        }

        public static string SpacesOrTabsOnly(string x) {
            string result = "";
            for (int i = 0; i < x.Length; i++) {
                if ((x[i] == ' ') | (x[i] == '\t')) {
                    result += x[i];
                }
            }
            return result;
        }
        public static string Untab(string code, string tabs) {
            if (string.IsNullOrEmpty(tabs)) return code;

            StringBuilder stringBuilder = new StringBuilder();
            using (StringReader reader = new StringReader(code)) {
                string line = string.Empty;
                do {
                    line = reader.ReadLine();
                    if (line != null) {
                        if (line.StartsWith(tabs))
                            line = line.Substring(tabs.Length);
                        stringBuilder.AppendLine(line);
                    }

                } while (line != null);
            }
            return stringBuilder.ToString();
        }
    }

}