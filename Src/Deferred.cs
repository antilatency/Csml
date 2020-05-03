using System;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace Csml {
    public class Deferred : Element<Deferred> {
        Func<IElement> Getter;
        public Deferred(Func<IElement> getter) {
            Getter = getter;
        }
        public override IEnumerable<HtmlNode> Generate(Context context) {
            return Getter().Generate(context);
        }
    }

}