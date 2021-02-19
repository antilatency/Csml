using Htmlilka;
using System.Globalization;

namespace Csml {
    public sealed class LanguageSideMenu : LanguageSideMenu<LanguageSideMenu> { }

    public class LanguageSideMenu<T> : Container<T> where T : LanguageSideMenu<T> {
        public LanguageSideMenu() : base("nav", "LanguageSideMenu") { }


        public override Node Generate(Context context) {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            var result = base.Generate(context) as Tag;

            foreach (var l in Language.All) {
                context.Language = l;
                var e = context.CurrentMaterial.Generate(context) as Tag;
                e.ChildrenNotNull.Clear();
                e.AddText(textInfo.ToUpper(l.Name));
                e.Attribute("title", "");
                //e.Attribute("onclick", "this.href += window.location.hash;");
                result.Add(e);
            }
            result = new Tag("div").AddClasses("LanguageSideMenuWrapper")
                .Add(new Tag("div").AddClasses("Tongue"))
                .Add(result);
            return result.Add(new Behaviour("LanguageSideMenu").Generate(context));
        }


    }
}