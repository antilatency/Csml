using System.Collections.Generic;
using HtmlAgilityPack;

namespace Csml {
    public class Grid :Collection<Grid> {
        Behaviour Behaviour;
        public Grid(int elementWidthPx, params int[] multipliers){
            Behaviour = new Behaviour("Grid", elementWidthPx, multipliers);
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            yield return HtmlNode.CreateNode("<div>").Do(x=>{
                x.AddClass("grid");
                x.Add(Behaviour.Generate(context));
                x.Add(base.Generate(context));
                
            });
        }
    }
}


