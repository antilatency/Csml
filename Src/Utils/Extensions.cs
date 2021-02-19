namespace Csml {
    public static class Extensions {
        public static T If<T>(this T _this, bool condition, System.Func<T, T> True) => condition ? True(_this) : _this;


    }
}
