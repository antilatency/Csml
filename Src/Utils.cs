using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
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
            using var sha = SHA1.Create();
            var h = sha.ComputeHash(data);
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

        public static T Single<T>(this IEnumerable<T> source) {
            if (source.Count() == 1) {
                return source.First();
            }
            throw new ArgumentException("Use of Single assumes that collection contains only one element.");
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
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

        public static PropertyInfo[] GetPropertiesAll(this Type x) {
            return x.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }
        public static bool ImplementsInterface(this Type x, Type interfaceType) {
            return x.GetInterfaces().Contains(interfaceType);
        }


        public static void CreateDirectory(string directory) {
            var last = Path.GetDirectoryName(directory);
            if (!Directory.Exists(last)) CreateDirectory(last);
            DirectoryInfo directoryInfo = Directory.CreateDirectory(directory);
            
            while (!directoryInfo.Exists) {
                Thread.Sleep(10);
            }
            
        }


        public static void DeleteDirectory(string directory) {
            if (Directory.Exists(directory)) {
                Directory.Delete(directory, true);
                while (Directory.Exists(directory)) {
                    Thread.Sleep(10);
                }
            }
                
        }

        

        public static Exception GetDeepestException(Exception e) {
            if (e.InnerException == null) {
                return e;
            } else {
                return GetDeepestException(e.InnerException);
            }
        }

        public static string ThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            return sourceFilePath;
        }
        public static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public static Color Lerp(this Color s, Color t, float k) {
            var bk = (1 - k);
            var a = s.A * bk + t.A * k;
            var r = s.R * bk + t.R * k;
            var g = s.G * bk + t.G * k;
            var b = s.B * bk + t.B * k;
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        public static uint ToRgba(this Color color) {
            var argb = (uint)color.ToArgb();
            uint rgba = argb << 8;
            rgba |= argb >> 24;
            return rgba;
        }
        public static uint ToRgb(this Color color) {
            var argb = (uint)color.ToArgb();
            uint rgba = argb & 0xFFFFFF;
            return rgba;
        }
    }


}