using System.Net;

namespace Csml {
    public sealed class GitHubCode : GitHubCode<GitHubCode> { }

    public class GitHubCode<T> : Container<T> where T : GitHubCode<T> {
        public GitHubCode():base("code","code"){
            WebClient webClient = new WebClient();
            var code = webClient.DownloadString("https://raw.githubusercontent.com/antilatency/Antilatency.com/master/Program.cs");
        
        }
    }
}