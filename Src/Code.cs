using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Csml {
    public sealed class Code : Code<Code> {
        public Code(string source = "") : base(source) {
        }
    }




    public class Code<T> : Container<T> where T : Code<T> {

        public string Source { get; set; }

        public static T LoadFromFile(string filePath, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "") {
            var fullPath = "";
            if (Path.IsPathRooted(filePath)) {
                fullPath = filePath;
            } else {
                fullPath = Path.GetFullPath(filePath, Path.GetDirectoryName(callerFilePath));
            }
            var source = File.ReadAllText(fullPath);
            return (T)Activator.CreateInstance(typeof(T), new object[] { source });
        }
        public static T LoadFromUri(string uri) {
            WebClient webClient = new WebClient();
            var source = webClient.DownloadString(uri);
            return (T)Activator.CreateInstance(typeof(T), new object[] { source });
        }

        public Code(string source) :base("pre","code"){
            Source = source;
        }

        public T GetClass(string className) {
            return GetClass("", className);
        }

        public static string Untab(SyntaxNode node) {
            var codeFragment = node.ToFullString();
            if (node.HasLeadingTrivia) {
                var tabs = node.GetLeadingTrivia().ToString();
                StringBuilder stringBuilder = new StringBuilder();
                using (StringReader reader = new StringReader(codeFragment)) {
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
            } else {
                return codeFragment;
            }            
        }

        public T GetClass(string namespaceName, string className) {
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);
            var root = syntaxTree.GetRoot();
            SyntaxList<MemberDeclarationSyntax> members = (SyntaxList<MemberDeclarationSyntax>)root.GetType().GetProperty("Members")?.GetValue(root);
            if (members == null) {
                Log.Error.OnCaller($"Invalid code fragment.");
            }
            ClassDeclarationSyntax c = null;

            if (!string.IsNullOrEmpty(namespaceName)) {
                foreach (var n in members.OfType<NamespaceDeclarationSyntax>().Where(x => x.Name.ToString() == namespaceName)) {
                    c = n.Members.OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText == className).FirstOrDefault();
                    if (c != null) break;
                }

            } else { 
                c = members.OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText == className).FirstOrDefault();
            }
            if (c == null) {
                Log.Error.OnCaller($"Class {className} not found.");
            }
            return (T)Activator.CreateInstance(typeof(T), new object[] { Untab(c) });
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return base.Generate(context).Single().Do(x=>x.InnerHtml = Source);
        }


        public T GetMethod(string methodName) {
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);
            var root = syntaxTree.GetRoot();
            SyntaxList<MemberDeclarationSyntax> members = (SyntaxList<MemberDeclarationSyntax>)root.GetType().GetProperty("Members")?.GetValue(root);

            var m = members.OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ValueText == methodName).FirstOrDefault();
            if (m == null) {
                Log.Error.OnCaller($"Methid {methodName} not found.");
            }


            return (T)Activator.CreateInstance(typeof(T), new object[] { Untab(m) });
        }
    }
}