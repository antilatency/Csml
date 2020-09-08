using Htmlilka;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Csml.Server {
    public class ServerState {
        public Dictionary<Scope, Dictionary<string, Dictionary<Language, PropertyInfo>>> ScopedMaterials { get; private set; }
        private object Sync = new object();

        public class Page {
            public string Hash;
            public string Content;
        }
        private Dictionary<string, Page> _pagesCache = new Dictionary<string, Page>();

        public ServerState() {
            if (ScopedMaterials == null) {
                var context = new Context();
                ScopedMaterials = new Dictionary<Scope, Dictionary<string, Dictionary<Language, PropertyInfo>>>();

                var s = ScopeHelper.All.ToArray();
                foreach (var scope in s) {
                    ScopedMaterials.Add(scope, scope.GenerateMaterialTypesMatrix(context));
                }
            }
        }

        public static string GetPageRefreshScript(string pageUrl, string pageHash, string sassHash, string javascriptHash, int refreshIntervalMs = 200) {
            return
                $"function WebServer_CheckNeedRefresh() {{" +
                $"  let xhr = new XMLHttpRequest();" +
                $"  xhr.open(\"POST\", \"/api/v1/refresh_required\");" +
                $"  xhr.onload = function() {{" +
                $"      if (xhr.status != 200) {{" +
                $"          console.log(`Page refresh query error: ${{xhr.status}}: ${{xhr.statusText}}`);" +
                $"          setTimeout(WebServer_CheckNeedRefresh, {refreshIntervalMs});" +
                $"      }} else {{" +
                $"          if(xhr.response == \"true\"){{" +
                $"              console.log(\"Refreshing page...\");" +
                $"              location.reload(true);" +
                $"          }}else{{" +
                $"              setTimeout(WebServer_CheckNeedRefresh, {refreshIntervalMs});" +
                $"          }}" +
                $"      }}" +
                $"  }};" +
                $"  xhr.onerror = function() {{" +
                $"      console.log(\"Page refresh query failed\");" +
                $"      setTimeout(WebServer_CheckNeedRefresh, {refreshIntervalMs});" +
                $"  }};" +
                $"  xhr.setRequestHeader('Content-Type', 'application/json');" +
                $"  var requestBody = {{}};" +
                $"  requestBody[\"SassHash\"] = \"{sassHash}\";" +
                $"  requestBody[\"JavaScriptHash\"] = \"{javascriptHash}\";" +
                $"  requestBody[\"PageUrl\"] = \"{pageUrl}\";" +
                $"  requestBody[\"PageHash\"] = \"{pageHash}\";" +
                $"  xhr.send(JSON.stringify(requestBody));" +
                $"}}" +
                $"setTimeout(WebServer_CheckNeedRefresh, {refreshIntervalMs});";
        }

        public static Tag FindFirstTag(Tag tag, string name) {
            if (!string.IsNullOrEmpty(tag.Name) && tag.Name.ToLowerInvariant() == name) {
                return tag;
            }
            foreach (var node in tag.Children) {
                if (node is Tag) {
                    var result = FindFirstTag(node as Tag, name);
                    if (result != null) {
                        return result;
                    }
                }
            }
            return null;
        }

        private static string PageToString(Tag page) {
            StringBuilder stringBuilder = new StringBuilder();
            page.WriteHtml(stringBuilder);
            return stringBuilder.ToString();
        }

        private static void PatchPage(Tag page, string pageUrl, string pageHash, string sassHash, string javascriptHash) {
            var head = FindFirstTag(page, "head");
            head.Add(new Tag("script").AddText(GetPageRefreshScript(pageUrl, pageHash, sassHash, javascriptHash)));
        }

        private string _lastSassHash;
        private string _lastJavaScriptHash;

        public Page GetPage(string url) {
            lock (Sync) {
                //Clear page cache on any file change (because files hashes embedded in cached page body)
                if ((_lastSassHash != FilesWatcher.SassHash) || (_lastJavaScriptHash != FilesWatcher.JavascriptHash)) {
                    _lastSassHash = FilesWatcher.SassHash;
                    _lastJavaScriptHash = FilesWatcher.JavascriptHash;
                    _pagesCache.Clear();
                    //Log.Info.Here("Cear page cache!");
                }

                Page page = null;
                if (_pagesCache.TryGetValue(url, out page)) {
                    return page;
                }

                var pageDom = GeneratePage(url);
                var pageHash = Hash.CreateFromString(PageToString(pageDom)).ToString();

                PatchPage(pageDom, url, pageHash, _lastSassHash, _lastJavaScriptHash);

                page = new Page {
                    Content = PageToString(pageDom),
                    Hash = pageHash
                };

                _pagesCache.Add(url, page);
                //Log.Info.Here("Cache updated for URL: " + url + " Records in cache: " + _pagesCache.Count);
                return page;
            }
        }

        public Htmlilka.Tag GeneratePage(string url) {
            foreach (var scopedMaterial in ScopedMaterials) {
                var scope = scopedMaterial.Key;
                var materialsMatrix = scopedMaterial.Value;
                foreach (var materialsLanguageGroup in materialsMatrix) {
                    foreach (var v in materialsLanguageGroup.Value) {
                        if (Material.GetUri(v.Key, v.Value) == url) {
                            Context context = new Context();
                            context.Language = v.Key;
                            return scope.GetTemplate().GenerateDom(context, v.Value.GetValue(scope) as IMaterial);
                        }
                    }
                }
            }
            return null;
        }
    }
}
