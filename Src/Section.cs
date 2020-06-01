using HtmlAgilityPack;
using System.Collections.Generic;
using System;

namespace Csml {
    public sealed class Section : Section<Section> {
        /// <summary>
        /// <param  name="identifier"/>
        /// Null - identifier not defined. Identifier will be created from title
        /// Empty - identifier set as "deleted". Html will not contains an identifier for that element
        /// </param>
        /// </summary>
        public Section(string title, string identifier = null) : base(title, identifier) {
        }
    }

    public interface ISection {
        public string Title { get; set; }
        public string Id { get; }
    }


    public class Section<T> : Collection<T>, ISection where T : Section<T> {
        public string Title { get; set; }
        public string Id {
            get {
                if (UserDefinedIdentifier != null) return UserDefinedIdentifier;
                else return Title.Replace(" ", "_");
            }
        }        
        public string UserDefinedIdentifier { get; set; }


        protected Section(string title, string identifier) {
            Title = title;
            UserDefinedIdentifier = identifier;
        }

        public override IEnumerable<HtmlNode> Generate(Context context) {

            var section = HtmlNode.CreateNode("<div>");
            section.AddClass("section");

            section.Do(x => {
                x.Add($"<h2>", "title").Do(x => {
                    if (Id != "")
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