using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using HtmlAgilityPack;

namespace Csml {
    public static class Utils {

        public static FormattableString Simplify(this FormattableString formattableString) {
            var format = formattableString.Format;
            var arguments = formattableString.GetArguments();
            var filteredArguments = new List<IElement>();
            int filteredArgumentID = -1;
            for (int i = 0; i < arguments.Length; i++) {
                if (arguments[i] is IElement) {
                    filteredArguments.Add(arguments[i] as IElement);
                    filteredArgumentID++;

                    if (i != filteredArgumentID) {
                        format = format.Replace("{" + i + "}", "{" + filteredArgumentID + "}");
                    }
                } else {
                    format = format.Replace("{" + i + "}", arguments[i].ToString());
                }
            }
            return FormattableStringFactory.Create(format, filteredArguments.ToArray());
        }

        internal static string ToHashString(byte[] data) {
            var h = SHA1.Create().ComputeHash(data);
            return Convert.ToBase64String(h).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }


        /*public static IElement FormattableStringToElement(FormattableString formattableString) {
            return new Text(formattableString);
        }*/

        /*public static IElement ObjectToElement(object obj) {
            if (obj is IElement) return obj as IElement;
            if 
            return new Text(formattableString);
        }*/


        public static string ToHtml(this IEnumerable<HtmlNode> nodes) {
            return string.Join("", nodes.Select(x => x.OuterHtml));
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source)
                action(item);
        }

        public static HtmlNode Add(this HtmlNode x, string html) {
            return x.AppendChild(HtmlNode.CreateNode(html));            
        }
        


        public static void CreateDirectories(string directory) {
            var first = Path.GetFileName(directory);
            var last = Path.GetDirectoryName(directory);
            if (!Directory.Exists(last)) CreateDirectories(last);
            Directory.CreateDirectory(directory);
        }


        public static class Static {
            public static string Class(string name) {
                return $"class = \"{name}\"";
            }
        }

    }


}