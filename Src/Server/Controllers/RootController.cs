using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
        private readonly ILogger<RootController> _logger;
        private readonly ServerState _state;

        public static string RootPath = @"/Root/Index_en.html";
        public RootController(ILogger<RootController> logger, ServerState state) {
            _logger = logger;
            _state = state;
        }

        public class RefreshRequest {
            public string SassHash { get; set; }
            public string JavaScriptHash { get; set; }
            public string PageUrl { get; set; }
            public string PageHash { get; set; }
        }

        [Route("/api/v1/refresh_required")]
        [HttpPost]
        public IActionResult GetRefreshRequired([FromBody] RefreshRequest request) {
            bool updateRequired = false;
            if((request.SassHash != FilesWatcher.SassHash) || (request.JavaScriptHash != FilesWatcher.JavascriptHash)) {
                //Log.Info.Here("Files updated!");
                updateRequired = true;
            } else if(_state.GetPage(request.PageUrl)?.Hash != request.PageHash) {
                //Log.Info.Here("Page content updated!");
                updateRequired = true;
            }
            return Content(updateRequired ? "true" : "false");
        }

        [Route("/{**catchAll}")]
        [HttpGet]
        public IActionResult GetContent() {
            var requestPath = ControllerContext.HttpContext.Request.Path;
            string resourcePath = (requestPath == "/" ? RootPath : requestPath.Value).Substring(1);
            var mimeType = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(resourcePath));

            //Procedural HTML page
            if(Path.GetExtension(resourcePath).ToLower() == ".html") {
                var material = _state.GetPage(CsmlApplication.WwwRootUri + resourcePath);
                if(material != null) {
                    return Content(material.Content, mimeType);

                }

            }

            //File
            resourcePath = Path.Combine(CsmlApplication.WwwRootDirectory, resourcePath);

            if(System.IO.File.Exists(resourcePath)) {
                Response.Headers.Add("Cache-Control", "no-cache");
                return PhysicalFile(resourcePath, mimeType);
            }

            Log.Warning.Here("NOT FOUND: " + resourcePath);
            Response.StatusCode = 404;
            return Content(null);
        }
    }
}
