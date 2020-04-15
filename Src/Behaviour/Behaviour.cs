using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csml;
using HtmlAgilityPack;

namespace Csml {
    public class Behaviour : Element<Behaviour> {
        private string UserDefinedClassName;
        public string ClassName {
            get {
                if (!string.IsNullOrEmpty(UserDefinedClassName)) return UserDefinedClassName;
                if (PropertyInfo == null) {
                    Log.Error.OnObject(this, "ClassName undefined");
                }
                return PropertyInfo.Name;
            }
        }
        private object[] Parameters;

        public Behaviour(string className = "", params object[] parameters) {
            UserDefinedClassName = className;
            Parameters = parameters;
        }

        public static string MakeParameterList(object[] parameters) {
            return string.Join(",", parameters.Select(x => {
                if (x.GetType() == typeof(string)) {
                    return "\"" + (string)x + "\"";
                }
                return x.ToString();
            }));
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode("<script>").Do(x => {
                var parameters = MakeParameterList(Parameters);
                var code = $"Behaviour.Initialize(\"{ClassName}.Create\"";
                if (!string.IsNullOrWhiteSpace(parameters))
                    code += "," + parameters;
                code += ")";
                x.InnerHtml = code;
            });
            //<script>Behaviour.Initialize("ToggleButton.Create",8,"string")</script>
            //return base.Generate(context);
        }
    }
}