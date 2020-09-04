using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Csml;
using Htmlilka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Csml.Server.Controllers {

    [ApiController]
    public class RootController : ControllerBase {
        private static Dictionary<Scope, Dictionary<string, Dictionary<Language, PropertyInfo>>> ScopedMaterials;

        private readonly ILogger<RootController> _logger;
        public RootController(ILogger<RootController> logger) {
            _logger = logger;
        }

        private void InitMaterials() {
            if (ScopedMaterials == null) {
                var context = new Context();
                ScopedMaterials = new Dictionary<Scope, Dictionary<string, Dictionary<Language, PropertyInfo>>>();

                var s = ScopeHelper.All.ToArray();
                foreach (var scope in s) {
                    ScopedMaterials.Add(scope, scope.GenerateMaterialTypesMatrix(context));
                }
            }
        }

        public static string GetPageRefreshScript(int refreshIntervalMs = 200) {
            return
                $"function WebServer_CheckNeedRefresh() {{" +
                $"  let xhr = new XMLHttpRequest();" +
                $"  xhr.open(\"GET\", \"/api/v1/refresh_required/{FilesWatcher.UpdateId}\");" +
                $"  xhr.onload = function() {{" +
                $"      if (xhr.status != 200) {{" +
                $"          console.log(`Page refresh query error: ${{xhr.status}}: ${{xhr.statusText}}`);" +
                $"      }} else {{" +
                $"          if(xhr.response == \"true\"){{" +
                $"              console.log(\"Refreshing page...\");" +
                $"              location.reload(true);" +
                $"          }}" +
                $"      }}" +
                $"  }};" +
                $"  xhr.onerror = function() {{" +
                $"      console.log(\"Page refresh query failed\");" +
                $"  }};" +
                $"  xhr.send();" +
                $"}}" +
                $"setInterval(WebServer_CheckNeedRefresh, {refreshIntervalMs});";
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

        private static void PatchPage(Tag page) {
            var head = FindFirstTag(page, "head");
            head.Add(new Tag("script").AddText(GetPageRefreshScript()));
        }

        private Htmlilka.Tag GetPage(string url) {
            foreach (var scopedMaterial in ScopedMaterials) {
                var scope = scopedMaterial.Key;
                var materialsMatrix = scopedMaterial.Value;
                foreach (var materialsLanguageGroup in materialsMatrix) {
                    foreach (var v in materialsLanguageGroup.Value) {
                        if (Material.GetUri(v.Key, v.Value) == url) {
                            Context context = new Context();
                            context.Language = v.Key;
                            var dom = scope.GetTemplate().GenerateDom(context, v.Value.GetValue(scope) as IMaterial);
                            PatchPage(dom);
                            return dom;
                        }
                    }
                }
            }
            return null;
        }

        [Route("/api/v1/refresh_required/{id:int}")]
        [HttpGet]
        public IActionResult GetRefreshRequired(int id) {
            return Content(id == FilesWatcher.UpdateId ? "false" : "true");
        }

        [Route("/{**catchAll}")]
        [HttpGet]
        public IActionResult GetContent() {
            InitMaterials();

            var requestPath = ControllerContext.HttpContext.Request.Path;
            string resourcePath = (requestPath == "/" ? @"/Root/Index_en.html" : requestPath.Value).Substring(1);

            var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(resourcePath));

            //Procedural HTML page
            if (Path.GetExtension(resourcePath).ToLower() == ".html") {
                var material = GetPage(CsmlApplication.WwwRootUri + resourcePath);
                StringBuilder stringBuilder = new StringBuilder();
                material.WriteHtml(stringBuilder);
                return Content(stringBuilder.ToString(), mimeType);
            }

            //File
            resourcePath = Path.Combine(CsmlApplication.WwwRootDirectory, resourcePath);

            if (System.IO.File.Exists(resourcePath)) {
                Response.Headers.Add("Cache-Control", "no-cache");
                return PhysicalFile(resourcePath, mimeType);
            }

            //Unknown
            Log.Warning.Here("FILE NOT FOUND: " + resourcePath);
            return NotFound();
        }
    }
}
