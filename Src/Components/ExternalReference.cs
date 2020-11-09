using Htmlilka;
using System;

namespace Csml {
    public class ExternalReference : Element<ExternalReference> {
        private string Href { get; set; }
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
        //TODO: validate
        private void SetHref(string href) {
            Href = href;
            /*try {
                Href = new Uri(href);
            } catch {
                Log.Error.OnCaller("Invalid href " + href);
            }*/
        }

        public override Node Generate(Context context) {
            Tag result;
            if (!context.AForbidden) {
                result = new Tag("a");
                result.Attribute("href", Href);
            } else {
                result = new Tag("span");
            }

            if (Image != null) {
                result.Add(Image.Generate(context));
            } else if (string.IsNullOrEmpty(Text)) {
                result.AddText(Href);
            } else {
                result.AddText(Text);
            }

            if (!string.IsNullOrEmpty(Tooltip)) {
                result.Attribute("title", Tooltip);
            } else {
                if (string.IsNullOrEmpty(Tooltip) & !string.IsNullOrEmpty(Text)) {
                    result.Attribute("title", Href);
                }
            }

            return result;

        }
    }

}