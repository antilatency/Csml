using Htmlilka;

namespace Csml {
    public sealed class LanguageMenu : LanguageMenu<LanguageMenu> { }

    public class LanguageMenu<T> : Container<T> where T : LanguageMenu<T> {
        public LanguageMenu() : base("nav", "language-menu") { }


        public override Node Generate(Context context) {
            var result = base.Generate(context) as Tag;

            foreach (var l in Language.All) {
                context.Language = l;
                var e = context.CurrentMaterial.Generate(context) as Tag;
                e.ChildrenNotNull.Clear();
                e.AddText(l.FullName);
                e.Attribute("onclick", "this.href += window.location.hash;");
                result.Add(e);
            }

            return result;
        }


    }
}