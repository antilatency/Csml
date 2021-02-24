using Htmlilka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Csml {

    public class DefaultTemplate : DefaultTemplate<DefaultTemplate> {
        public DefaultTemplate() : base () { }
    }
    public class DefaultTemplate<T> : Template<T> where T : DefaultTemplate<T> {
        public DefaultTemplate(){
        }

        public override void ModifyBody(Tag x, Context context, IMaterial material) {
            base.ModifyBody(x, context, material);

        }
    }
}
