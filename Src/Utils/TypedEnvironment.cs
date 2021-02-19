using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Csml {
    public class TypedEnvironment {
        public TypedEnvironment() {
            var variables = Environment.GetEnvironmentVariables();

            var type = this.GetType();
            foreach(var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                if(variables.Contains(f.Name)) {
                    f.SetValue(this, Convert.ChangeType(variables[f.Name], f.FieldType));
                }
            }

        }
    }
}
