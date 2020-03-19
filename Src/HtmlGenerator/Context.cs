using System.Text;
using System.Reflection;
using System.IO;
using System;
using HtmlAgilityPack;

using System.Collections.Generic;
using System.Linq;

namespace Csml {




    public class Context {

        public string SourceRootDirectory { get; set; }
        public string OutputRootDirectory { get; set; }

        public Uri BaseUri { get; set; }

        public string SubDirectory { get; set; }

        public Language Language { get; set; }
        public HtmlDocument Page { get; set; }



        public Context() {
        }

        public Context Copy() {
            return new Context(this);
        }

        protected Context(Context other) {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x=>x.CanWrite & x.CanRead);
            foreach (var p in properties) {
                p.SetValue(this, p.GetValue(other));
            }
        }


        


        
        public Context IncrementSubDirectory(string value) {
            SubDirectory =
                GetContentRelativePath(
                    Path.GetFullPath(Path.Combine(SourceRootDirectory, SubDirectory, value)),
                    SourceRootDirectory
                );
            return this;
        }
        public string GetSubDirectoryFromSourceAbsoluteDiectory(string directory) {
            return GetContentRelativePath(directory, SourceRootDirectory);

        }
        public string GetSubDirectoryFromSourceAbsoluteFilePath(string path) {
            return GetContentRelativePath(Path.GetDirectoryName(path), SourceRootDirectory);
        }


        public Context BeginPage(Action<HtmlDocument> modifyPage) {

            Page = new HtmlDocument();
            Page.DocumentNode.AppendChild(HtmlNode.CreateNode("<!DOCTYPE html>"));
            var html = Page.DocumentNode.AppendChild(Page.CreateElement("html"));
            var head = html.AppendChild(Page.CreateElement("head"));
            
            //<BASE>
            //head.AppendChild(HtmlNode.CreateNode($"<base href=\"{BaseUri}\">"));


            var body = html.AppendChild(Page.CreateElement("body"));

            if (AutoReload) {
                head.AppendChild(HtmlNode.CreateNode("<meta http-equiv=\"refresh\" content=\"1\">"));
            }

            modifyPage(Page);
            return this;
        }

        public Context EndPage(string outputFilePath) {
            if (string.IsNullOrEmpty(outputFilePath)) {
                throw new ArgumentException("outputFilePath is empty");
            }
            CreateDirectories(OutputDirectory);

            var outputFileAbsolutePath = Path.Combine(OutputDirectory, outputFilePath);

            Page.Save(outputFileAbsolutePath);

            Page = null;
            return this;
        }


        private static string GetContentRelativePath(string absolutePath, string basePath) {
            if (!absolutePath.StartsWith(basePath)) {
                throw new ArgumentException($"{absolutePath} is not subpath of {basePath}");
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