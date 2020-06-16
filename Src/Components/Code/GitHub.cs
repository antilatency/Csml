using Octokit;
using System;
using System.Net;

namespace Csml {
    namespace GitHub {
        public class Owner {
            public string Name { get; set; }
            public Owner(string name) {
                Name = name;
            }
            public RepositoryBranch GetRepositoryBranch(string repositoryName, string reference = "master") {
                return new RepositoryBranch(this, repositoryName, reference);
            }

            public RepositoryBranch GetRepositoryBranchPinned(string repositoryName, string reference = "master") {
                if (RepositoryBranch.IgnorePinning)
                    return new RepositoryBranch(this, repositoryName, reference);

                var client = new GitHubClient(new ProductHeaderValue("com.antilatency.csml"));
                var sha = client.Repository.Commit.GetSha1(Name, repositoryName, reference).Result;
                return new RepositoryBranch(this, repositoryName, sha);
            }
        }

        public class RepositoryBranch {
            public static bool IgnorePinning = false;
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

        [CacheConfig("GitHubCache", false)]
        public class FileCache : Cache<FileCache> {
            public string Content;
        }

        public class File {
            public RepositoryBranch RepositoryBranch { get; set; }
            public string Path { get; set; }
            private FileCache FileCache;
            //private string Content { get; set; }
            public File(RepositoryBranch repositoryBranch, string path,
                [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0
                ) {
                RepositoryBranch = repositoryBranch;
                Path = path;
                var hash = RepositoryBranch.Owner.Name + RepositoryBranch.Name + RepositoryBranch.Reference + path;
                hash = string.Concat(hash.Split(System.IO.Path.GetInvalidFileNameChars()));
                //using (new Stopwatch("Load hash of " + hash)) {
                    FileCache = FileCache.Load(hash);
                    if (FileCache == null) {
                        FileCache = FileCache.Create(hash);
                        var client = new WebClient();
                        try {
                            FileCache.Content = client.DownloadStringTaskAsync(RawUri).Result;
                        }
                        catch (Exception e) {
                            Log.Error.On(callerFilePath, callerLineNumber, e.Message);
                        }
                        FileCache.Save();
                    }
                //}
                //var
                
            }
            private string GitHubCom => "https://github.com";
            private string RawGitHubCom => "https://raw.githubusercontent.com";

            public string HtmlUri => $"{GitHubCom}/{RepositoryBranch.Owner.Name}/{RepositoryBranch.Name}/blob/{RepositoryBranch.Reference}/{Path}";
            public string RawUri => $"{RawGitHubCom}/{RepositoryBranch.Owner.Name}/{RepositoryBranch.Name}/{RepositoryBranch.Reference}/{Path}";

            public override string ToString() {
                return FileCache.Content;
            }
        }

    }
}