using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Csml {
    public class CsmlApplication {
        public static string ProjectRootDirectory { get; internal set; }

        public static string WwwRootDirectory { get; internal set; }

        public static Uri WwwRootUri { get; internal set; }

        public static string PageTitlePrefix { get; internal set; }

        public static bool IsDeveloperMode { get; internal set; }

        public static SassProcessor SassProcessor { get; internal set; }

        public static JavascriptProcessor JavascriptProcessor { get; internal set; }

        public static List<IMaterial> SiteMapMaterials { get; internal set; } = new List<IMaterial>();

        public static Uri WwwCssUri => SassProcessor != null ? new Uri(WwwRootUri, SassProcessor.OutputFileName) : null;
        public static Uri WwwJsUri => JavascriptProcessor != null ? new Uri(WwwRootUri, JavascriptProcessor.OutputFileName) : null;

        private static Matcher CleanupMatcher = null;

        private CsmlApplication() { }

        public static void ReleaseBuild(string projectRootDirectory, string wwwRootDirectory, Uri wwwRootUri) {
            ProjectRootDirectory = projectRootDirectory;
            WwwRootDirectory = wwwRootDirectory;
            WwwRootUri = wwwRootUri;
            PageTitlePrefix = string.Empty;
            IsDeveloperMode = false;
            SiteMapMaterials = new List<IMaterial>();
            CleanupMatcher = GetCleanupMatcherForReleaseBuild(wwwRootDirectory);

            Build();
        }

        public static void DeveloperBuild(string projectRootDirectory, string wwwRootDirectory, Uri wwwRootUri, bool watch = false) {
            ProjectRootDirectory = projectRootDirectory;
            WwwRootDirectory = wwwRootDirectory;
            WwwRootUri = wwwRootUri;
            PageTitlePrefix = F5.Prefix;
            IsDeveloperMode = true;
            SiteMapMaterials = new List<IMaterial>();
            CleanupMatcher = null;

            GitHub.RepositoryBranch.IgnorePinning = true;
            ToDo.Enabled = true;

            Build();

            if (watch) {
                Log.Info.Here($"DeveloperBuild: Watching for file change (*.scss, *.js)...");
                Watch();
            } else {
                F5.Send();
            }
        }

        public static void WatchJsCssWithoutBuild(string projectRootDirectory, string wwwRootDirectory, Uri wwwRootUri) {
            ProjectRootDirectory = projectRootDirectory;
            WwwRootDirectory = wwwRootDirectory;
            WwwRootUri = wwwRootUri;
            PageTitlePrefix = F5.Prefix;
            IsDeveloperMode = true;
            SiteMapMaterials = new List<IMaterial>();
            CleanupMatcher = null;

            SetupCache();
            CreateFileProcessors();

            Watch();
        }

        private static void Build() {
            Log.Info.Here($"Csml builder: Enable Scope Properties Caching");
            ScopeHelper.EnableGetOnce();

            Log.Info.Here($"Csml builder: Setup Cache");
            SetupCache();

            Log.Info.Here($"Csml builder: Cleanup Output Directory");
            CleanupOutputDirectory();

            Log.Info.Here($"Csml builder: Fonts;Sass;Javascript");
            CopyFonts();
            CreateFileProcessors();

            Log.Info.Here($"Csml builder: Generate...");
            var context = new Context();
            ScopeHelper.All.ForEach(x => { x.Generate(context); });

            Log.Info.Here($"Csml builder: Create SiteMap");
            CreateSiteMap(SiteMapMaterials);

            Log.Info.Here($"Csml builder: Build Done!");
        }

        private static void SetupCache() {
            CacheConfig.PrivateCacheDirectory = Path.Combine(ProjectRootDirectory, ".cache");

            if (IsDeveloperMode) {
                CacheConfig.PublicCacheDirectory = CacheConfig.PrivateCacheDirectory;
                CacheConfig.PublicCacheUri = null;
            } else {
                CacheConfig.PublicCacheDirectory = WwwRootDirectory;
                CacheConfig.PublicCacheUri = WwwRootUri;
            } 
        }

        private static void CleanupOutputDirectory() {
            var cleanupDirectory = WwwRootDirectory;

            if (CleanupMatcher != null) {
                var files = CleanupMatcher.GetResultsInFullPath(cleanupDirectory);

                foreach (var file in files) {
                    File.Delete(file);
                }

                Utils.DeleteEmptySubdirectories(cleanupDirectory);
            } else {
                if (!Directory.Exists(cleanupDirectory)) {
                    Utils.CreateDirectory(cleanupDirectory);
                    return;
                }

                foreach (var subDir in Directory.GetDirectories(cleanupDirectory)) {
                    Utils.DeleteDirectory(subDir);
                }

                foreach (var file in Directory.GetFiles(cleanupDirectory)) {
                    File.Delete(file);
                }
            }
        }

        private static void CopyFonts() {
            var sourceDirectory = Path.Combine(ProjectRootDirectory, "Fonts");
            var destDirectory = Path.Combine(WwwRootDirectory, "Fonts");
            var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

            var extensions = new List<string>() { ".ttf", ".woff", ".woff2", ".svg", ".eot" };
            var fonts = files.Where(x => extensions.Contains(Path.GetExtension(x)));

            foreach (var f in fonts) {
                var relativePath = Path.GetRelativePath(sourceDirectory, f);
                var destPath = Path.Combine(destDirectory, relativePath);

                if (File.Exists(destPath)) {
                    continue;
                }

                var subDirectory = Path.GetDirectoryName(relativePath);
                Utils.CreateDirectory(Path.Combine(destDirectory, subDirectory));
                File.Copy(f, destPath);
            }
        }

        private static void CreateFileProcessors() {
            SassProcessor = new SassProcessor(IsDeveloperMode, ProjectRootDirectory, WwwRootDirectory, "Style.scss");
            JavascriptProcessor = new JavascriptProcessor(IsDeveloperMode, ProjectRootDirectory, WwwRootDirectory);
        }

        private static void CreateSiteMap(IEnumerable<IMaterial> materials) {
            if (materials != null) {
                var languages = Language.All;

                var map = new StringBuilder()
                        .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                        .AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">");

                foreach (var material in materials) {
                    foreach (var l in languages) {
                        map.AppendLine("\t<url>");
                        map.AppendLine($"\t<loc>{material.GetUri(l)}</loc>");

                        foreach (var l2 in languages) {
                            map.AppendLine($"\t\t<xhtml:link rel=\"alternate\" hreflang=\"{l2.Name}\" href=\"{material.GetUri(l2)}\"/>");
                        }
                        map.AppendLine("\t</url>");
                    }
                }
                map.AppendLine("</urlset>");

                File.WriteAllText(Path.Combine(WwwRootDirectory, "SiteMap.xml"), map.ToString());
            }
        }

        private static Matcher GetCleanupMatcherForReleaseBuild(string directory) { 
            var csmlDoNotDeleteFileName = "csmlDoNotDelete.json";
            var csmlDoNotDeletePath = Path.Combine(directory, csmlDoNotDeleteFileName);

            var matcher = new Matcher()
                .AddInclude("**/*")
                .AddExclude("index.html")
                .AddExclude("**/.*")
                .AddExclude(csmlDoNotDeleteFileName);

            if (File.Exists(csmlDoNotDeletePath)) {
                var customIgnoreList = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(Utils.ReadAllText(csmlDoNotDeletePath));
                if (customIgnoreList != null) {
                    foreach (var i in customIgnoreList) {
                        matcher.AddExclude(i);
                    }
                }
            }

            return matcher;
        }

        private static void Watch() {
            bool reloadRequired = true;
            string ScssError = null;

            while (Console.KeyAvailable == false) {
                if (reloadRequired) {
                    F5.Send();
                    reloadRequired = false;
                }
                if (SassProcessor.Error != ScssError) {
                    ScssError = SassProcessor.Error;
                    Console.Clear();
                    if (ScssError != null) {
                        Console.WriteLine("Scss:" + ScssError);
                    }
                }

                System.Threading.Thread.Sleep(250);

                reloadRequired |= SassProcessor.UpdateIfChanged();
                reloadRequired |= JavascriptProcessor.UpdateIfChanged();
            }
        }
    }
}
