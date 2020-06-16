using System;
using System.Collections.Generic;
using System.Text;

namespace Csml {
    public interface IMaterial : IElement {
        string Title { get; }
        Image TitleImage { get; }
        IElement Description { get; }
        IElement Content { get; }
        string GetPath(Language language);
        string GetUri(Language language);
    }
}
