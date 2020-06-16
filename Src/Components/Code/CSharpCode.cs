using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Csml {

    public sealed class CSharpCode : CSharpCode<CSharpCode> {
        
        public CSharpCode(string source) : base(CSharpSyntaxTree.ParseText(source).GetRoot()){
            Source = source;
        }
        public CSharpCode(GitHub.File source) : base(CSharpSyntaxTree.ParseText(source.ToString()).GetRoot()) {
            Source = source;
        }
        public CSharpCode(SyntaxNode parsedSource) : base(parsedSource) {}
        public CSharpCode(CSharpCode other, SyntaxNode parsedSource) : base(other, parsedSource) {}

    }
    

    public class CSharpCode<T> : Code<T> where T : CSharpCode<T> {
        protected override ProgrammingLanguage GetProgrammingLanguage() => ProgrammingLanguage.CSharp;
        public SyntaxNode ParsedSource;

        public CSharpCode(SyntaxNode parsedSource) {
            ParsedSource = parsedSource;
        }

        public CSharpCode(T other, SyntaxNode parsedSource) : base(other) {
            ParsedSource = parsedSource;
        }

        protected override string GetFinalSourceCode() {
            return Untab(ParsedSource);
        }

        protected override System.Range? GetLineSpan() {
            if (ParsedSource.Span.Start == 1)
                if (ParsedSource.Span.End == ParsedSource.SyntaxTree.Length)
                    return null;

            var span = ParsedSource.SyntaxTree.GetLineSpan(ParsedSource.Span);
            return new System.Range(span.StartLinePosition.Line, span.EndLinePosition.Line);
        }

        public T GetClass(string className) {
            return GetClass("", className);
        }        

        public static string Untab(SyntaxNode node) {
            var codeFragment = node.ToString();
            if (node.HasLeadingTrivia) {                
                var tabs = SpacesOrTabsOnly(node.GetLeadingTrivia().ToString());
                return Untab(codeFragment, tabs);
            } else {
                return codeFragment;
            }            
        }

        public T GetClass(string namespaceName, string className,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0
            ) {
            SyntaxList<MemberDeclarationSyntax> members = (SyntaxList<MemberDeclarationSyntax>)ParsedSource.GetType().GetProperty("Members")?.GetValue(ParsedSource);
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
                Log.Error.On(callerFilePath, callerLineNumber, $"Class {className} not found.");
            }

            var result = (T)Activator.CreateInstance(typeof(T),this,c);
            return result;
        }
        
        public T GetMethod(string methodName,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0) {
            SyntaxList<MemberDeclarationSyntax> members = (SyntaxList<MemberDeclarationSyntax>)ParsedSource.GetType().GetProperty("Members")?.GetValue(ParsedSource);

            var m = members.OfType<MethodDeclarationSyntax>().Where(x => x.Identifier.ValueText == methodName).FirstOrDefault();
            if (m == null) {
                Log.Error.On(callerFilePath, callerLineNumber, $"Method {methodName} not found.");
            }

            var result = (T)Activator.CreateInstance(typeof(T), this, m);
            return result;
        }

        


        
    }
}