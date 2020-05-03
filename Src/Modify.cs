using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public static class ModifyStatic {
        public static Modify Modify(this IElement x) {
            return new Modify(x);
        }
    }

    public class Modify : Element<Modify> {
        private IElement Element;
        Func<Context, IEnumerable<HtmlNode>> Modifier;
        //public Func<Context, IEnumerable<HtmlNode>> ModifierClone => (Func<Context, IEnumerable<HtmlNode>>)Modifier.Clone();
        
        public Modify(FormattableString formattableString): this(new Text(formattableString)){
            
        }

        public Modify(IElement element = null) {
            if (element == null) {
                Element = new Element(context => HtmlNode.CreateNode("<span>"));
            } else {
                Element = element;
            }            
            Modifier = (context) => Element.Generate(context);
        }

        public Modify ContentReplace(FormattableString replacement) {
            return ContentReplace(new Text(replacement));
        }
        public Modify ContentReplace(IElement replacement) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Visit(x => {
                    x.InnerHtml = "";
                    x.Add(replacement.Generate(context));
                });
            return this;
        }
        public HtmlNode WrapTextToSpan(HtmlNode x) {
            if (x.Name == "#text") {
                return HtmlNode.CreateNode($"<span>{x.InnerText}</span>");
            }
            return x;
        }

        public Modify AddClasses(params string[] classes) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Select(x => {
                    x = WrapTextToSpan(x);                    
                    foreach (var c in classes)
                        x.AddClass(c);
                    return x;
                });
            return this;
        }

        public Modify SetAttributeValue(string name, string value) {
            var prevModifier = Modifier;
            Modifier = (context) =>
                prevModifier(context).Select(x => {
                    x = WrapTextToSpan(x);
                    x.SetAttributeValue(name, value);
                    return x;
                });
            return this;
        }

        public Modify WrapIfMany(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                var previous = prevModifier(context);
                var count = previous.Count();
                if (count == 1) return previous;
                return new HtmlNode[] {
                    HtmlNode.CreateNode($"<{tag}>").Add(previous)
                };
            };
            return this;
        }

        public Modify Wrap(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                return new HtmlNode[] {
                    HtmlNode.CreateNode($"<{tag}>").Add(prevModifier(context))
                };
            };
            return this;
        }

        public Modify Tag(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => prevModifier(context).Visit(x => {
                x.Name = tag;
                }
                ) ;
            return this;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {
            var generated = Modifier(context);
            foreach (var g in generated) yield return g;
        }

    }

}