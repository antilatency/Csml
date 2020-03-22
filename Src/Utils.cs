using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static HtmlNode Wrap(this HtmlNode x, string html) {
            var result = HtmlNode.CreateNode(html);
            result.AppendChild(x);
            return result;
        }

        public static PropertyInfo[] GetPropertiesPublicStatic(this Type x) {
            return x.GetProperties(BindingFlags.Public | BindingFlags.Static);
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

            public static string ThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
                return sourceFilePath;
            }
            public static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
                return Path.GetDirectoryName(sourceFilePath);
            }

            public static Dictionary<object, object> BackupStorage;
            public static T Backup<T>(Func<T> func) {
                if (BackupStorage == null) BackupStorage = new Dictionary<object, object>();
                if (BackupStorage.ContainsKey(func)) {
                    return (T)BackupStorage[func];
                }
                T result = func();
                BackupStorage.Add(func, result);
                return (T)result;
            }

        }

    }


}