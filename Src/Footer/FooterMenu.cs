using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace Csml {
    public sealed class FooterMenu : Container<FooterMenu> {
        public FooterMenu() : base("nav", "FooterMenu") { }
    }

    public sealed class FooterMenuSection : Collection<FooterMenuSection> {
        public string Title { get; private set; }
        public FooterMenuSection(string title) {
            Title = title;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var section = HtmlNode.CreateNode("<div>");

            section.AddClass("FooterMenuSection");

            if (!string.IsNullOrEmpty(Title)) {
                section.AppendChild(HtmlNode.CreateNode($"<h5 style=\"margin-bottom: 12px;\">{Title}</h5>"));
            }

            section.Add(base.Generate(context));

            yield return section;
        }
    }

    public sealed class FooterMenuSectionList : ContainerWithSpecificChildren<FooterMenuSectionList> {
        public FooterMenuSectionList() : base("ul", "li", "FooterMenuSectionList") {

        }
    }
}
