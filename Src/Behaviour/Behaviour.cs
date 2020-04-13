using System.Collections.Generic;
using System.IO;
using Csml;
using HtmlAgilityPack;

namespace Csml {
    public class Behaviour : Element<Behaviour> {
        private string Method;
        private object[] Parameters;

        public Behaviour(string scriptFullPath, params object[] parameters) {
            if (!File.Exists(scriptFullPath))
                Log.Error.OnCaller($"File {scriptFullPath} not found");

            var scriptName = Path.GetFileNameWithoutExtension(scriptFullPath);
            Method = $"{scriptName}.Create";
            Parameters = parameters;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            return base.Generate(context);
        }
    }
}