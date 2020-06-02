using HtmlAgilityPack;
using System.Collections.Generic;
using System;

namespace Csml {

    public class ToDo : Element<ToDo>{
        public static bool Enabled { get; set; } = false;
        public string Text { get; private set; }
        public bool ShowText { get; private set; }
        public ToDo(string text, bool showText = false, bool suppressWarning = false) {            
            Text = text;
            ShowText = showText;
            if (!suppressWarning) Log.ToDo.OnObject(this, text);
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (!Enabled)
                yield break;

            yield return HtmlNode.CreateNode("<span>").Do(x=> {
                x.AddClass("ToDo");
                if (ShowText) {
                    x.Add(Text);
                }
                x.SetAttributeValue("title", Text);
            });
        }
    }
}