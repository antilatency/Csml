
using Htmlilka;

namespace Csml
{
    public sealed class Spoiler : Spoiler<Spoiler>
    {
        public Spoiler(string title, string identifier = null) : base(title, identifier)
        {
        }
    }

    public class Spoiler<T> : Collection<T>, ISection where T : Spoiler<T>
    {
        public string Title { get; set; }
        public string Id
        {
            get
            {
                if (UserDefinedIdentifier != null) return UserDefinedIdentifier;
                else return Title.Replace(" ", "_");
            }
        }
        public string UserDefinedIdentifier { get; set; }


        protected Spoiler(string title, string identifier){
            Title = title;
            UserDefinedIdentifier = identifier;
        }

        public override Node Generate(Context context){
            var details = new Tag("details");
            details.AddClasses("Spoiler");

            details.AddTag("summary", x => {
                x.AddClasses("SpoilerSummary");
                x.AddText(Title);                
            });
            details.Add(base.Generate(context));

            return details;
        }
    }
}