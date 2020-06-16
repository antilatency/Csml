
namespace Csml {
    public partial class CsmlPredefined : Scope {
        public static LanguageSelector<IMaterial> NoTarget => new LanguageSelector<IMaterial>();
        public static Material NoTarget_en => new Material("This page does not exist yet", null, $"This page does not exist yet");
        public static Material NoTarget_ru => new Material("Этой страницы пока не существует", null, $"Этой страницы пока не существует");

    }
}