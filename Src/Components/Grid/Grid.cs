using Htmlilka;

namespace Csml {
    public class Grid :Collection<Grid> {
        Behaviour Behaviour;
        public Grid(int elementWidthPx, params int[] multipliers){
            Behaviour = new Behaviour("Grid", elementWidthPx, multipliers);
        }

        public override Node Generate(Context context) {
            return new Tag("div").AddClasses("Grid")
                .Add(Behaviour.Generate(context))
                .Add(base.Generate(context));
        }
    }
}


