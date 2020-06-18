using System;
using System.Collections.Generic;
using System.Text;

namespace Csml {
    public class CsmlWorkspace {

        private static CsmlWorkspace _instance;
        internal static CsmlWorkspace Current => _instance ??= new CsmlWorkspace();

        public string ProjectRootDirectory { get; internal set; }

        public string WwwRootDirectory { get; internal set; }

        public Uri WwwRootUri { get; internal set; }

        public string PageTitlePrefix { get; internal set; }

        public bool IsDeveloperMode { get; internal set; }

        public SassProcessor SassProcessor { get; internal set; }

        public JavascriptProcessor JavascriptProcessor { get; internal set; }

        public List<IMaterial> SiteMapMaterials { get; internal set; } = new List<IMaterial>();

        public Uri WwwCssUri => SassProcessor != null ? new Uri(WwwRootUri, SassProcessor.OutputFileName) : null;
        public Uri WwwJsUri => JavascriptProcessor != null ? new Uri(WwwRootUri, JavascriptProcessor.OutputFileName) : null;

        private CsmlWorkspace() {  }
    }
}
