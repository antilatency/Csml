namespace Csml {
    public interface IPageTemplate {
        Htmlilka.Tag GenerateDom(Context context, IMaterial material);
        void Generate(Context context, IMaterial material);
    }
}