using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

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
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);
            var root = syntaxTree.GetRoot();
            var members = root.GetType().GetProperty("Members")?.GetValue(root);
            if (members == null) {
                Log.Error.OnCaller($"Invalid code fragment.");
            }
            var membersTyped = (SyntaxList<MemberDeclarationSyntax>)members;
            var c = membersTyped.OfType<ClassDeclarationSyntax>().Where(x => x.Identifier.ValueText == className).FirstOrDefault();
            if (c == null) {
                Log.Error.OnCaller($"Class {className} not found.");
            }

            return (T)Activator.CreateInstance(typeof(T), new object[] { c.ToString() });
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return base.Generate(context).Single().Do(x=>x.InnerHtml = Source);
        }


        public T GetMethod(CompilationUnitSyntax compilationUnit, string className, string methodName) {
            var syntaxTree = CSharpSyntaxTree.ParseText(Source);
            var root = syntaxTree.GetRoot();

            var c = compilationUnit.Members.OfType<ClassDeclarationSyntax>().Where(x=>x.Identifier.ValueText == className).FirstOrDefault();
            if (c == null) {
                Log.Error.OnCaller($"Class {className} not found.");
            }
            var m = c.Members.OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ValueText == methodName).FirstOrDefault();
            if (m == null) {
                Log.Error.OnCaller($"Methid {methodName} not found.");
            }
            return new Code<T>(m.ToString()) as T;
        }
    }
}