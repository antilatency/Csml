using Htmlilka;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csml {
    public static class ModifyStatic {
        public static Modify Modify(this IElement x) {
            return new Modify(x);
        }
    }

    public class Modify : Element<Modify> {
        Func<Context, Node> Modifier;        
        public Modify(FormattableString formattableString): this(new Text(formattableString)){
            
        }

        public Modify(IElement element = null) {
            if (element == null) {
                element = new Element(context => new Tag("span"));
            }        
            Modifier = (context) => element.Generate(context);
        }

        public Modify ContentReplace(FormattableString replacement) {
            return ContentReplace(new Text(replacement));
        }
        public Modify ContentReplace(IElement replacement) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                var result = prevModifier(context);
                if (result is TextNode) {
                    result = replacement.Generate(context);
                } else {
                    if (result is Tag tag) {
                        tag.Children.Clear();
                        tag.Add(replacement.Generate(context));
                    } else {
                        Log.Error.Here($"ContentReplace error: invalid node type {result.GetType().Name}");
                    }
                }
                return result;
            };
            return this;
        }


        public Modify AddClasses(params string[] classes) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                var result = prevModifier(context);
                if (result is VoidTag voidTag) {
                    voidTag.AddClasses(classes);
                } else {
                    Log.Error.Here($"AddClasses error: invalid node type {result.GetType().Name}");
                }
                return result;
            };
            return this;
        }

        public Modify Attribute(string name, string value) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                var result = prevModifier(context);
                if (result is VoidTag voidTag) {
                    voidTag.Attribute(name, value);
                } else {
                    Log.Error.Here($"Attribute error: invalid node type {result.GetType().Name}");
                }
                return result;
            };

            return this;
        }

        public Modify Wrap(string tag) {
            var prevModifier = Modifier;
            Modifier = (context) => {
                return new Tag(tag).Add(prevModifier(context));
            };
            return this;
        }


        public override Node Generate(Context context) {
            return Modifier(context);
        }

    }

}