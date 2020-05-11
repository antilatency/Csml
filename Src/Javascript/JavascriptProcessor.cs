
using System.Collections.Generic;
using System.IO;
using System.Text;

class JavascriptProcessor : FileProcessor {

    public JavascriptProcessor(string sourceRootDirectory, string outputRootDirectory) :
        base(sourceRootDirectory, outputRootDirectory) {
        Error = Update();
    }
    IEnumerable<string> GetFilePathes() {
        return Directory.GetFiles(SourceRootDirectory, "*.js", SearchOption.AllDirectories);
    } 
    public override bool IsChanged() { 
        if (base.IsChanged()) return true;

        var files = GetFilePathes();
        foreach (var f in files) {
            if (!observableFiles.ContainsKey(f)) return true;
        }

        return false;
    }

    protected override string Update() {
        observableFiles = CaptureModificationTimes(GetFilePathes());
        NUglify.JavaScript.CodeSettings codeSettings = new NUglify.JavaScript.CodeSettings();
        //NUglify.SymbolMap
        using (StreamWriter writer = new StreamWriter(Path.Combine(OutputRootDirectory, "script.map"))) {

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var i in observableFiles.Keys) {
                
                var js = Csml.Utils.ReadAllText(i);

                var u = NUglify.Uglify.Js(js, i);

                stringBuilder.AppendLine(u.Code);

            }

            File.WriteAllText(Path.Combine(OutputRootDirectory, "script.js"),stringBuilder.ToString());
        }
        
        return null;
    }
}