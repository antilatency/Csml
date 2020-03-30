

namespace Csml {

    public class Template {
        public IElement Pre { get; set; }
        public IElement Post { get; set; }
        public Template(IElement pre, IElement post) {
            Pre = pre;
            Post = post;
        }
    }
}