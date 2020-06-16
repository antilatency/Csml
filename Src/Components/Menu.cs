namespace Csml {
    public sealed class Menu : Menu<Menu> { }

    public class Menu<T> : Container<T> where T:Menu<T>{
        public Menu() : base("nav","menu") { }
    }
}