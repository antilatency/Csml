
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Csml {
    public class JavascriptProcessor : FileProcessor {
        public string OutputFileName { get; private set; }

        public JavascriptProcessor(bool developerMode, string sourceRootDirectory, string outputRootDirectory) :
            base(developerMode, sourceRootDirectory, outputRootDirectory)
        {
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
            var pathes = GetFilePathes();

            if (DeveloperMode) {
                observableFiles = CaptureModificationTimes(pathes);
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var i in pathes) {

                var js = Csml.Utils.ReadAllText(i);
                var u = NUglify.Uglify.Js(js, i);
                stringBuilder.AppendLine(u.Code);
            }

            var result = stringBuilder.ToString();
            OutputFileName = "script.js";
            if (!DeveloperMode) {
                OutputFileName = Hash.CreateFromString(result).ToString() + ".js";
            }

            File.WriteAllText(Path.Combine(OutputRootDirectory, OutputFileName), result);


            return null;
        }
    }
}