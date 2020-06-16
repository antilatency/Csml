using System.IO;
using System;
using HtmlAgilityPack;

namespace Csml {

    public struct Context {

        public bool AForbidden;
        public string FormatString;

        public Language Language { get; set; }
        public HtmlDocument CurrentHtmlDocument { get; set; }
        public IMaterial CurrentMaterial { get; set; }
        public HtmlNode Head { get; set; }

        public void BeginPage() {
            CurrentHtmlDocument = new HtmlDocument();
            CurrentHtmlDocument.OptionUseIdAttribute = true;
            CurrentHtmlDocument.DocumentNode.AppendChild(HtmlNode.CreateNode("<!DOCTYPE html>"));
            var html = CurrentHtmlDocument.DocumentNode.AppendChild(CurrentHtmlDocument.CreateElement("html"));
            Head = html.AppendChild(CurrentHtmlDocument.CreateElement("head"));

            var body = html.AppendChild(CurrentHtmlDocument.CreateElement("body"));
        }

        public void EndPage(string path) {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("outputFilePath is empty");
            }
            Utils.CreateDirectory(Path.GetDirectoryName(path));
            //var outputFileAbsolutePath = Path.Combine(OutputDirectory, outputFilePath);
            CurrentHtmlDocument.Save(path);
            CurrentHtmlDocument = null;
        }


        public static string GetContentRelativePath(string absolutePath, string basePath) {
            if (!absolutePath.StartsWith(basePath)) {
                throw new ArgumentException($"{absolutePath} is not subpath of {basePath}");
            }
            return Path.GetRelativePath(basePath, absolutePath);
        }
    }

}