using System.Collections.Generic;
using HtmlAgilityPack;

namespace Csml {
    public sealed class LanguageMenu : LanguageMenu<LanguageMenu> { }

    public class LanguageMenu<T> : Container<T> where T : LanguageMenu<T> {
        public LanguageMenu() : base("nav", "language-menu") { }


        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return base.Generate(context).Single().Do(x => {
                foreach (var l in Language.All) {
                    context.Language = l;
                    x.AppendChild(context.CurrentMaterial.Generate(context).Single().Do(x => x.InnerHtml = l.FullName));
                }
            });
        }


    }
}