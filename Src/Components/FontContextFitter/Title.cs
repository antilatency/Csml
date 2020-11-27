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
            Text = separationPattern == "" ? text : Separator(text, separationPattern);
            H = h;
        }

        private string Separator(string text, string pattern) {
            var separator = pattern.Replace(" ", "");
            return pattern.IndexOf(" ") == 0 ? text.Replace(separator, "<wbr/>" + separator) : text.Replace(separator, separator + "<wbr/>");
        }

        public override Node Generate(Context context) => new Tag("h" + H)
            .AddClasses("Title")
            .AddPureHtmlNode(Text)
            .Add(new Behaviour("FontContextFitter").Generate(context));
    }
}
