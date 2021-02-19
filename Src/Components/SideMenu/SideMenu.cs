using Htmlilka;

namespace Csml {
    public sealed class SideMenu : SideMenu<SideMenu> { }

    public class SideMenu<T> : Container<T> where T:SideMenu<T>{
        public SideMenu() : base("nav","SideMenu") { }

        public override Node Generate(Context context) {
            return new Tag("div").AddClasses("SideMenuWrapper")
                .Add(new Tag("div").AddClasses("TongueMenu"))
                .Add(base.Generate(context))
                .Add(new Behaviour("SideMenu").Generate(context))
            ;
        }
    }
}