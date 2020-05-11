using System.Text;
using System.Reflection;
using System.IO;
using System;
using HtmlAgilityPack;

using System.Collections.Generic;
using System.Linq;

namespace Csml {




    public struct Context {
        //public bool Watch { get; set; }
        //public string SourceRootDirectory { get; set; }
        //public string OutputRootDirectory { get; set; }

        //public string WatchPrefix => Watch? "f5me.":"";

        public HashSet<string> AssetsToCopy {
            get;
            set;
        }



        //public Uri BaseUri { get; set; }

        //public string SubDirectory { get; set; }

        public Language Language { get; set; }
        public HtmlDocument CurrentHtmlDocument { get; set; }
        public IMaterial CurrentMaterial { get; set; }
        public HtmlNode Head { get; set; }






        /*protected Context(Context other) {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x=>x.CanWrite & x.CanRead);
            foreach (var p in properties) {
                p.SetValue(this, p.GetValue(other));
            }
        }*/

        /*public void CleanOutputRootDirectory() {
            Utils.DeleteDirectory(OutputRootDirectory);            
        }

        public void CopyAssets() {
            foreach (var i in AssetsToCopy) {
                var dest = Path.Combine(OutputRootDirectory, Context.GetContentRelativePath(i, SourceRootDirectory));
                Utils.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(i, dest, true);
            }
        }*/

        /*public string GetSubDirectoryFromSourceAbsoluteDiectory(string directory) {
            return GetContentRelativePath(directory, SourceRootDirectory);

        }*/
        /*public string GetSubDirectoryFromSourceAbsoluteFilePath(string path) {
            return GetContentRelativePath(Path.GetDirectoryName(path), SourceRootDirectory);
        }*/


        public void BeginPage() {

            CurrentHtmlDocument = new HtmlDocument();
            CurrentHtmlDocument.OptionUseIdAttribute = true;
            CurrentHtmlDocument.DocumentNode.AppendChild(HtmlNode.CreateNode("<!DOCTYPE html>"));
            var html = CurrentHtmlDocument.DocumentNode.AppendChild(CurrentHtmlDocument.CreateElement("html"));
            Head = html.AppendChild(CurrentHtmlDocument.CreateElement("head"));
            
            //<BASE>
            //head.AppendChild(HtmlNode.CreateNode($"<base href=\"{BaseUri}\">"));


            var body = html.AppendChild(CurrentHtmlDocument.CreateElement("body"));

            /*if (AutoReload) {
                Head.AppendChild(HtmlNode.CreateNode("<meta http-equiv=\"refresh\" content=\"1\">"));
            }*/


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


        //public string SourceDirectory => Path.Combine(SourceRootDirectory, SubDirectory);
        //public string OutputDirectory => Path.Combine(OutputRootDirectory, SubDirectory);


        //public bool ForceRebuildAssets { get; set; } = true;
        //public bool ForceRebuildImages { get; set; } = false;

        //public bool AutoReload { get; set; }


    }

}