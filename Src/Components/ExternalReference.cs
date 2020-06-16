using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace Csml {
    public class ExternalReference : Element<ExternalReference> {
        private Uri Href { get; set; }
        private string Text { get; set; }
        private string Tooltip { get; set; }


        public ExternalReference(string href, string text = "", string tooltip = "") {
            try {
                Href = new Uri(href);
            }
            catch {
                Log.Error.OnCaller("Invalid href " + href);
            }
            Text = text;
            Tooltip = tooltip;
        }


        public override IEnumerable<HtmlNode> Generate(Context context) {

            yield return HtmlNode.CreateNode(context.AForbidden ? "<span>" : "<a>").Do(x => {
                x.AddClass("Text");
                if (!context.AForbidden)
                    x.SetAttributeValue("href", Href.ToString());

                if (string.IsNullOrEmpty(Text))
                    x.InnerHtml = Href.ToString();
                else
                    x.InnerHtml = Text;
                if (!string.IsNullOrEmpty(Tooltip)) {
                    x.SetAttributeValue("title", Tooltip);
                } else {
                    if (string.IsNullOrEmpty(Tooltip) & !string.IsNullOrEmpty(Text)) {
                        x.SetAttributeValue("title", Href.ToString());
                    }
                }
            });

        }
    }

}