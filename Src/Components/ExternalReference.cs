using HtmlAgilityPack;
using System;
using System.Collections.Generic;

namespace Csml {
    public class ExternalReference : Element<ExternalReference> {
        private Uri Href { get; set; }
        private string Text { get; set; }
        private Image Image { get; set; }
        private string Tooltip { get; set; }


        public ExternalReference(string href, string text = "", string tooltip = "") {
            SetHref(href);
            Text = text;
            Image = null;
            Tooltip = tooltip;
        }

        public ExternalReference(string href, Image image, string tooltip = "") {
            SetHref(href);
            Text = string.Empty;
            Image = image;
            Tooltip = tooltip;
        }

        private void SetHref(string href) {
            try {
                Href = new Uri(href);
            } catch {
                Log.Error.OnCaller("Invalid href " + href);
            }
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {

            yield return HtmlNode.CreateNode(context.AForbidden ? "<span>" : "<a>").Do(x => {
                x.AddClass("Text");
                if (!context.AForbidden)
                    x.SetAttributeValue("href", Href.ToString());

                if (Image != null) {
                    x.Add(Image.Generate(context));
                } else if (string.IsNullOrEmpty(Text)) {
                    x.InnerHtml = Href.ToString();
                } else {
                    x.InnerHtml = Text;
                }
                    
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