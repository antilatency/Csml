using Htmlilka;

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

        public override Node Generate(Context context) {

            return new Tag("div")
                .AddClasses("Section")
                .AddTag("h2", x => {
                    x.AddClasses("Title");
                    x.AddText(Title);
                    if (Id != "") {
                        x.Attribute("id", Id);
                        if (!context.AForbidden) {
                            x.AddTag("a", a => {
                                a.AddClasses("Link");
                                a.Attribute("href", "#" + Id);
                                a.Attribute("title", "Heading anchor");
                            });
                        }
                    }
                })
                .Add(base.Generate(context))
                ;

        }
    }




}