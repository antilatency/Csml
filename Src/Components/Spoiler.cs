using HtmlAgilityPack;
using System.Collections.Generic;

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


        protected Spoiler(string title, string identifier)
        {
            Title = title;
            UserDefinedIdentifier = identifier;
        }

        public override IEnumerable<HtmlNode> Generate(Context context)
        {
            var details = HtmlNode.CreateNode("<details>");
            details.AddClass("Spoiler");
            details.Do(x => {
                x.Add($"<summary>", "SpoilerSummary").Do(x => {
                    x.Add(Title);
                });
                x.Add(base.Generate(context));
            });
            yield return details;
        }
    }
}