using System.Collections.Generic;
using System;
using Htmlilka;

namespace Csml {

    /// <summary>
    /// Font size decrementer to prevent scroll
    /// </summary>
    public class Title : Element<Title> {
        public string Text { get; set; }
        public int H { get; private set; }

        public Title(string text, string separationPattern = "", int h = 1) {
            if(separationPattern != "") {
                Text = Separator(text, separationPattern);
            } else {
                Text = text;
            }
            H = h;
        }

        private string Separator(string text, string pattern) {
            var separator = pattern.Replace(" ", "");
            if(pattern.IndexOf(" ") == 0) {
                return text.Replace(separator, "<wbr/>" + separator);
            }
            return text.Replace(separator, separator + "<wbr/>");
        }

        public override Node Generate(Context context) => new Tag("h" + H)
            .AddClasses("Title")
            .AddPureHtmlNode(Text)
            //.AddTag("h1", x => {
            //    x.AddClasses("Title");
            //    x.AddText(Title);
            //})
            .Add(new Behaviour("FontContextFitter").Generate(context));
    }
}
