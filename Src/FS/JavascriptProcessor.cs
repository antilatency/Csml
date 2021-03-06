
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
                var code = Utils.ReadAllText(i);

                var thisFilePath = Path.GetRelativePath(SourceRootDirectory, i).Replace('\\', '/');
                code = code.Replace("☺thisFilePath",thisFilePath);


                if (!DeveloperMode) {
                    var uglifyResult = NUglify.Uglify.Js(code, i);

                    if (uglifyResult.HasErrors) {
                        Console.WriteLine("Javascript Uglify error: " + uglifyResult.ToString());
                    } else {
                        code = uglifyResult.Code;
                    }
                }


                stringBuilder.AppendLine(code);
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