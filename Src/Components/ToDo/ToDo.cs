using System.Collections.Generic;
using System;
using Htmlilka;

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

        public override Node Generate(Context context) {
            if (!Enabled)
                return new Tag(null);

            var result = new Tag("span");
            if (ShowText) {
                result.AddText(Text);
            }
            result.AddClasses("ToDo");
            result.Attribute("title", Text);
            return result;
        }
    }
}