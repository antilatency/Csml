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
        public string Id { get; }
    }


    public class Section<T> : Collection<T>, ISection where T : Section<T> {
        public string Title { get; set; }
        public string Id => Title.Replace(" ","_");
        
        protected Section(string title) {
            Title = title;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {

            var section = HtmlNode.CreateNode("<div>");
            section.AddClass("section");

            section.Do(x => {
                x.Add($"<h2>", "title").Do(x => {
                    x.Id = Id;
                    x.Add(Title);
                    if (!context.AForbidden) {
                        x.Add("<a>", "link").Do(x => {
                            x.SetAttributeValue("href", "#" + Id);
                            x.SetAttributeValue("title", "Heading anchor");
                        });
                    }
                    
                });

                x.Add(base.Generate(context));                
            });

            yield return section;
        }
    }




}