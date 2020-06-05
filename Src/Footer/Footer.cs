using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Csml {
    public sealed class Footer : Collection<Footer>  {
        public Footer()  { }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var footer = HtmlNode.CreateNode("<footer>");
            var container = footer.AppendChild(HtmlNode.CreateNode("<div>"));

            footer.AddClass("Footer");
            container.AddClass("FooterContainer");

            container.Add(base.Generate(context));

            yield return footer;
        }
    }
}
