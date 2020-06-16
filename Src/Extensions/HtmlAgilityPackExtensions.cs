using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Csml {
    public static class HtmlAgilityPackExtensions {

        public static string ToHtml(this IEnumerable<HtmlNode> nodes) {
            return string.Join("", nodes.Select(x => x.OuterHtml));
        }

        public static HtmlNode AddTextWithWordBreaks(this HtmlNode x, string text) {
            var BreackingChars = @"\.";
            Regex regex = new Regex($@"[^{BreackingChars}]*[{BreackingChars}]?");
            int start = 0;
            while (true) {
                var m = regex.Match(text, start);
                start = m.Index + m.Value.Length;
                x.Add(m.Value);
                if (start == text.Length) break;
                x.Add("<wbr/>");
            }
            return x;
        }

        public static HtmlNode Add(this HtmlNode x, string html) {
            return x.AppendChild(HtmlNode.CreateNode(html));
        }

        public static HtmlNode Add(this HtmlNode x, IEnumerable<HtmlNode> nodes) {
            nodes.ForEach(c => x.AppendChild(c));
            return x;
        }

        public static HtmlNode Add(this HtmlNode x, string html, params string[] classes) {
            var result = HtmlNode.CreateNode(html);
            foreach (var c in classes) result.AddClass(c);
            return x.AppendChild(result);
        }

        public static HtmlNode Do(this HtmlNode x, Action<HtmlNode> action) {
            action(x);
            return x;
        }

        public static HtmlNode Wrap(this HtmlNode x, string html) {
            var result = HtmlNode.CreateNode(html);
            result.AppendChild(x);
            return result;
        }
    }
}
