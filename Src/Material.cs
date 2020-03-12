using System;

namespace Csml {
    public sealed class Material : Material<Material> {
        public Material(string title, object titleImage, FormattableString description
            ): base(title, titleImage, description) {
        }
    }

    public class Material<T> : Translatable<T>, ITranslatable where T : class , ITranslatable {
        public string Title { get; set; }
        public object titleImage;
        public FormattableString description;

        protected Material(string title, object titleImage, FormattableString description)  {
            Title = title;
            this.titleImage = titleImage;
            this.description = description;
        }
    }
}