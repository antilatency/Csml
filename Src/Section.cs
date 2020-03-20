using HtmlAgilityPack;
using System.Collections.Generic;
using System;

namespace Csml {
    public sealed class Section : Section<Section> {
        public Section(string title) : base(title) {
        }
    }

    public interface ISection {
        public string Title { get; set; }
        public string Anchor { get; }
    }


    public class Section<T> : Collection<T>, ISection where T : Section<T> {
        public string Title { get; set; }
        public string Anchor => Uri.EscapeDataString(Title.Replace(" ","_"));
        
        protected Section(string title) {
            Title = title;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {

            var root = HtmlNode.CreateNode($"<div id=\"{Anchor}\"></div>");
            //root.AppendChild(HtmlNode.CreateNode($"<span />"));
            root.AppendChild(HtmlNode.CreateNode($"<h3>{Title}</h3>"));
            foreach (var i in base.Generate(context)) {
                root.AppendChild(i);
            }
            yield return root;
        }
    }




}