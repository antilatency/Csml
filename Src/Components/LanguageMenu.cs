using Htmlilka;
using System.Globalization;

namespace Csml {
    public sealed class LanguageMenu : LanguageMenu<LanguageMenu> { }

    public class LanguageMenu<T> : Container<T> where T : LanguageMenu<T> {
        public LanguageMenu() : base("nav", "language-menu") { }


        public override Node Generate(Context context) {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            var result = base.Generate(context) as Tag;

            foreach (var l in Language.All) {
                context.Language = l;
                var e = context.CurrentMaterial.Generate(context) as Tag;
                e.ChildrenNotNull.Clear();
                e.AddText(textInfo.ToUpper(l.Name));
                e.Add(new Behaviour("LanguageMenu").Generate(context));
                //e.Attribute("onclick", "this.href += window.location.hash;");
                result.Add(e);
            }
            return result;
        }


    }
}