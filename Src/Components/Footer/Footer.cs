using System;
using System.Collections.Generic;
using System.Text;
using Htmlilka;

namespace Csml {
    public sealed class Footer : Collection<Footer>  {
        public Footer()  { }

        public override Node Generate(Context context) {

            return new Tag("footer").AddClasses("Footer")
                .AddDiv(a => {
                    a.AddClasses("FooterContainer");
                    a.Add(base.Generate(context));
                });
        }
    }
}
