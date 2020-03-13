using System.Text;
using System.Reflection;
using System.IO;
using System;
using HtmlAgilityPack;

namespace Csml {

    public class GeneratorContext {
        public static string ContentDirectory;
        public static string GetContentRelativePath(string absoluteContentPath) {
            if (!absoluteContentPath.StartsWith(ContentDirectory)) {
                throw new ArgumentException($"{absoluteContentPath} is nor subpath of {ContentDirectory}");
            }
            return Path.GetRelativePath(ContentDirectory, absoluteContentPath);
        }
        public static string CurrentOutputSubirectory;

        public void CreateDirectories(string directory) {
            var first = Path.GetFileName(directory);
            var last = Path.GetDirectoryName(directory);
            if (!Directory.Exists(last)) CreateDirectories(last);
            Directory.CreateDirectory(directory);

        }

    }

    public class HtmlGenerator<R> : GeneratorContext {
         

        /*static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }*/

        public void Generate(string outputDirectory) {
            var fields = typeof(R).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var f in fields) {
                var value = f.GetValue(null);
                if (value is IMaterial) {
                    var material = value as IMaterial;
                    var translatable = value as ITranslatable;

                    var subpath = GetContentRelativePath((value as ICallerInfo).CallerSourceFilePath);
                    CurrentOutputSubirectory = Path.GetDirectoryName(subpath);
                    CreateDirectories(Path.Combine(outputDirectory, CurrentOutputSubirectory));
                    var filePath = Path.Combine(outputDirectory, CurrentOutputSubirectory, f.Name + ".html");
                    //FileStream fileStream = new FileStream();

                    //XmlWriter xmlWriter = XmlWriter.Create(fileName);
                    //xmlWriter.WriteStartElement()

                    HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlDocument();
                    //
                    htmlDocument.DocumentNode.AppendChild(HtmlNode.CreateNode("<!DOCTYPE html>"));

                    var html = htmlDocument.DocumentNode.AppendChild(htmlDocument.CreateElement("html"));
                    html.SetAttributeValue("lang", translatable.Language.name);

                    var head = html.AppendChild(htmlDocument.CreateElement("head"));
                    var body = html.AppendChild(htmlDocument.CreateElement("body"));

                    var title = head.AppendChild(htmlDocument.CreateElement("title"));
                    title.AppendChild(htmlDocument.CreateTextNode(material.Title));

                    

                    htmlDocument.Save(filePath);


                    /*var html = htmlDocument. CreateElement("html");
                    html.SetAttribute("lang", translatable.Language.name);
                    var head = html.AppendChild(xmlDocument.CreateElement("head"));
                    var body = html.AppendChild(xmlDocument.CreateElement("body"));

                    var content = xmlDocument.InnerText;*/
                    //File.WriteAllText(Path.Combine(outputDirectory, CurrentOutputSubirectory, f.Name + ".html"), result);
                }


                //var result = OnObject(value);

                


                /*if (!string.IsNullOrEmpty(result)) {
                    
                    
                }*/
            }
        }

        public string OnObject(object value) {
            if (value is IMaterial) {
                return OnMaterial(value as IMaterial);
                
            }
            return null;
        }

        public string OnMaterial(IMaterial material) {
            return material.Title;
        }
    }
}