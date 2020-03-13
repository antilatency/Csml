using System;

namespace Csml {
    public sealed class Material : Material<Material> {
        public Material(string title, object titleImage, FormattableString description
            ): base(title, titleImage, description) {
        }
    }

    public interface IMaterial {
        public string Title { get; set; }
        public object TitleImage { get; set; }
    }
    

    public class Material<T> : Translatable<T>, IMaterial where T : Material<T> {
        public string Title { get; set; }
        public object TitleImage { get; set; }
        public FormattableString description;

        protected Material(string title, object titleImage, FormattableString description)  {
            Title = title;
            this.TitleImage = titleImage;
            this.description = description;
        }
    }
}