using System.Text;
using System.Reflection;
using System.IO;
using System;
using HtmlAgilityPack;

using System.Collections.Generic;
using System.Linq;

namespace Csml {

    public class Context {
        [ThreadStatic]
        private static Stack<Context> stack;

        public static Context Push() {
            Context context = new Context(stack.Peek());
            stack.Push(context);
            return context;
        }
        public static void Pop() {
            stack.Pop();
        }
        public static Context Current {
            get {
                if (stack == null) stack = new Stack<Context>();
                if (stack.Count == 0) {
                    stack.Push(new Context());
                }
                return stack.Peek();
            }
        }

        protected Context() {
            stack.Push(this);
        }

        protected Context(Context other) {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x=>x.CanWrite & x.CanRead);
            foreach (var p in properties) {
                p.SetValue(this, p.GetValue(other));
            }
        }


        public string SourceRootDirectory { get; set; }
        public string OutputRootDirectory { get; set; }


        public string SubDirectory { get; set; }
        public Context IncrementSubDirectory(string value) {
            SubDirectory =
                GetContentRelativePath(
                    Path.GetFullPath(Path.Combine(SourceRootDirectory, SubDirectory, value)),
                    SourceRootDirectory
                );
            return this;
        }
        public Context SetSubDirectoryFromSourceAbsoluteDiectory(string directory) {
            SubDirectory = GetContentRelativePath(directory, SourceRootDirectory);
            return this;
        }
        public Context SetSubDirectoryFromSourceAbsoluteFilePath(string path) {
            SubDirectory = GetContentRelativePath(Path.GetDirectoryName(path), SourceRootDirectory);
            return this;
        }

        public HtmlDocument Page { get; set; }
        public string OutputFileName { get; set; }

        public string OutputFileAbsolutePath => Path.Combine(OutputDirectory, OutputFileName);

        public Context BeginPage(string outputFileName, Action<HtmlDocument> modifyPage) {
            OutputFileName = outputFileName;

            Page = new HtmlDocument();
            Page.DocumentNode.AppendChild(HtmlNode.CreateNode("<!DOCTYPE html>"));
            var html = Page.DocumentNode.AppendChild(Page.CreateElement("html"));
            var head = html.AppendChild(Page.CreateElement("head"));
            var body = html.AppendChild(Page.CreateElement("body"));

            if (AutoReload) {
                head.AppendChild(HtmlNode.CreateNode("<meta http-equiv=\"refresh\" content=\"1\">"));
            }

            modifyPage(Page);
            return this;
        }

        public Context EndPage() {
            if (string.IsNullOrEmpty(OutputFileName)) {
                throw new ArgumentException("OutputFileName is empty");
            }
            CreateDirectories(OutputDirectory);
            Page.Save(OutputFileAbsolutePath);

            Page = null;
            OutputFileName = null;
            return this;
        }


        private static string GetContentRelativePath(string absolutePath, string basePath) {
            if (!absolutePath.StartsWith(basePath)) {
                throw new ArgumentException($"{absolutePath} is nor subpath of {basePath}");
            }
            return Path.GetRelativePath(basePath, absolutePath);
        }


        public string SourceDirectory => Path.Combine(SourceRootDirectory, SubDirectory);
        public string OutputDirectory => Path.Combine(OutputRootDirectory, SubDirectory);


        public static void CreateDirectories(string directory) {
            var first = Path.GetFileName(directory);
            var last = Path.GetDirectoryName(directory);
            if (!Directory.Exists(last)) CreateDirectories(last);
            Directory.CreateDirectory(directory);
        }


        public bool AutoReload { get; set; }


    }

}