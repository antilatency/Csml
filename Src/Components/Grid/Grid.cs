using Htmlilka;

namespace Csml {
    public class Grid : Collection<Grid> {
        //Behaviour Behaviour;
        int MinElementWidthPx;
        int MinColumns;

        public Grid(int minElementWidthPx, int minColumns = 1) {
            MinElementWidthPx = minElementWidthPx;
            MinColumns = minColumns;
        }
        public override Node Generate(Context context) {
            var css = @$"
                grid-template-columns: repeat(auto-fit, minmax({MinElementWidthPx}, 1fr));
                grid-template-columns: repeat(auto-fit, minmax( min({MinElementWidthPx}px, {100/MinColumns}% - { 8.0 * (MinColumns - 1) / MinColumns }px), 1fr));
            ";

            return new Tag("div").Attribute("style",css).AddClasses("Grid").Add(base.Generate(context));

        }
    }
}


