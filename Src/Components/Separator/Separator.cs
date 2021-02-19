using Htmlilka;

namespace Csml {
public class Separator : Element<Separator> {
        public Separator() : base() { }
        
        public override Node Generate(Context context) {
            return new Tag("div").AddClasses("Separator");
        }
    }
}