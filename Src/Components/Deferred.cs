using Htmlilka;
using System;

namespace Csml {
    public class Deferred : Element<Deferred> {
        readonly Func<IElement> Getter;
        public Deferred(Func<IElement> getter) {
            Getter = getter;
        }
        public override Node Generate(Context context) {
            return Getter().Generate(context);
        }
    }

}