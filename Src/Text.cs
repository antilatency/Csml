using System;

namespace Csml {
    public sealed class Text: Text<Text> {
        public Text(FormattableString formattableString) {
            Format = formattableString.Format;
            foreach (var a in formattableString.GetArguments()) {
                Add(a);
            }
        }
    }

    public class Text<T> : Collection<T> where T : Text<T> {
        public string Format;
    }
}