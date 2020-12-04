using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csml;
using Htmlilka;
using Newtonsoft.Json;

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


            return string.Join(",", parameters.Select(x => JsonConvert.SerializeObject(x)));
        }

        public override Node Generate(Context context) {
            
            var parameters = MakeParameterList(Parameters);
            var code = $"Behaviour.Initialize(\"{ClassName}\"";
            if (!string.IsNullOrWhiteSpace(parameters))
                code += "," + parameters;
            code += ")";

            return new Tag("script").AddCode(code);
        }
    }
}