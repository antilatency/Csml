using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Csml {
    public class CsmlBuilder {

        public CsmlWorkspace Workspace { get; private set; }

        private Matcher _cleanupMatcher = null;

        private CsmlBuilder() { }

        public static CsmlBuilder Create(string projectRootDirectory, string wwwRootDirectory, Uri wwwRootUri) {
            var ws = CsmlWorkspace.Current;

            ws.ProjectRootDirectory = projectRootDirectory;
            ws.WwwRootDirectory = wwwRootDirectory;
            ws.WwwRootUri = wwwRootUri;
            ws.PageTitlePrefix = "";
            ws.IsDeveloperMode = false;
            ws.SiteMapMaterials = new List<IMaterial>();

            return new CsmlBuilder() { Workspace = ws };
        }

        public CsmlBuilder SetPageTitlePrefix(string prefix) {
            Workspace.PageTitlePrefix = prefix;
            return this;
        }

        public CsmlBuilder SetDeveloperMode(bool developerMode) {
            Workspace.IsDeveloperMode = developerMode;
            return this;
        }

        public CsmlBuilder SetCleanupMatcher(Matcher matcher) {
            _cleanupMatcher = matcher;
            return this;
        }

        public void Build() {
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
            CreateSiteMap(Workspace.SiteMapMaterials);

            Log.Info.Here($"Csml builder: Done!");
        }

        private void SetupCache() {
            CacheConfig.PrivateCacheDirectory = Path.Combine(Workspace.ProjectRootDirectory, ".cache");

            if (Workspace.IsDeveloperMode) {
                CacheConfig.PublicCacheDirectory = CacheConfig.PrivateCacheDirectory;
                CacheConfig.PublicCacheUri = null;
            } else {
                CacheConfig.PublicCacheDirectory = Workspace.WwwRootDirectory;
                CacheConfig.PublicCacheUri = Workspace.WwwRootUri;
            } 
        }

        private void CleanupOutputDirectory() {
            var cleanupDirectory = Workspace.WwwRootDirectory;

            if (_cleanupMatcher != null) {
                var files = _cleanupMatcher.GetResultsInFullPath(cleanupDirectory);

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

        private void CopyFonts() {
            var sourceDirectory = Path.Combine(Workspace.ProjectRootDirectory, "Fonts");
            var destDirectory = Path.Combine(Workspace.WwwRootDirectory, "Fonts");
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

        private void CreateFileProcessors() {
            Workspace.SassProcessor = new SassProcessor(Workspace.IsDeveloperMode, Workspace.ProjectRootDirectory, Workspace.WwwRootDirectory, "Style.scss");
            Workspace.JavascriptProcessor = new JavascriptProcessor(Workspace.IsDeveloperMode, Workspace.ProjectRootDirectory, Workspace.WwwRootDirectory);
        }

        private void CreateSiteMap(IEnumerable<IMaterial> materials) {
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

                File.WriteAllText(Path.Combine(Workspace.WwwRootDirectory, "SiteMap.xml"), map.ToString());
            }
        }
    }
}
