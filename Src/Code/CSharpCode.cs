using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Csml {
    namespace GitHub {
        public class Owner {
            public string Name { get; set; }
            public Owner(string name) {
                Name = name;
            }
            public RepositoryBranch GetRepositoryBranch(string name, string reference = "master") {
                return new RepositoryBranch(this, name, reference);
            }

            public RepositoryBranch GetRepositoryBranchPinned(string repositoryName, string reference = "master") {
                var client = new GitHubClient(new ProductHeaderValue("com.antilatency.csml"));
                var sha = client.Repository.Commit.GetSha1(Name, repositoryName, reference).Result;
                return new RepositoryBranch(this, repositoryName, sha);
            }
        }

        public class RepositoryBranch {
            public Owner Owner { get; set; }
            public string Name { get; set; }
            public string Reference { get; set; }
            public RepositoryBranch(Owner owner, string name, string reference) {
                Owner = owner;
                Name = name;
                Reference = reference;
            }
            public File GetFile(string path) {
                return new File(this, path);
            }
        }

        public class File {
            public RepositoryBranch RepositoryBranch { get; set; }
            public string Path { get; set; }
            public string Content { get; set; }
            public File(RepositoryBranch repositoryBranch, string path,
                [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0
                ) {
                RepositoryBranch = repositoryBranch;
                Path = path;
                var client = new WebClient();

                try {
                    Content = client.DownloadStringTaskAsync(RawUri).Result;
                }
                catch (Exception e) {
                    Log.Error.On(callerFilePath, callerLineNumber, e.Message);
                }
            }
            private string GitHubCom => "https://github.com";
            private string RawGitHubCom => "https://raw.githubusercontent.com";

            public string HtmlUri => $"{GitHubCom}/{RepositoryBranch.Owner.Name}/{RepositoryBranch.Name}/blob/{RepositoryBranch.Reference}/{Path}";
            public string RawUri => $"{RawGitHubCom}/{RepositoryBranch.Owner.Name}/{RepositoryBranch.Name}/{RepositoryBranch.Reference}/{Path}";
            
            public override string ToString() {
                return Content;
            }
        }

    }
    


    /*public class GithubFile {
        public Task<IReadOnlyList<RepositoryContent>> content;
        public GithubFile(string owner, string repository, string path, string reference = "master") {
            var client = new GitHubClient(new ProductHeaderValue("com.antilatency.csml"));
            var rep = client.Repository.Commit.Get(owner, repository , reference).Result;
            var sha = client.Repository.Commit.GetSha1(owner, repository, reference).Result;
            content = client.Repository.Content.GetAllContentsByRef(owner, repository, path, sha);
        }
        public override string ToString() {
            return content.Result[0].Content;
        }
    }*/
    
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
        protected override ColorCode.ILanguage ProgrammingLanguage => ColorCode.Languages.CSharp;
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