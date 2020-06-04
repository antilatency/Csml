
using Csml;
using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

class SassProcessor : FileProcessor, IFileManager {
    public string OutputFileName { get; private set; }
    readonly string InitialFilePath;

    public SassProcessor(bool developerMode, string sourceRootDirectory, string outputRootDirectory, string initialFilePath):
        base(developerMode, sourceRootDirectory, outputRootDirectory) {
        
        if (Path.IsPathRooted(initialFilePath)) {
            InitialFilePath = initialFilePath;
        } else {
            InitialFilePath = Path.Combine(SourceRootDirectory, initialFilePath);
        }
        
        if (DeveloperMode) InitializeWriteTimes();
        Error = Update();
    } 

    void InitializeWriteTimes() {
        observableFiles = new Dictionary<string, DateTime> {
            { InitialFilePath, new DateTime() }
        };
    }


    public bool SupportsConversionToAbsolutePath => false;
    public bool FileExists(string path) {
        return File.Exists(path);
    }
    public string GetCurrentDirectory() {
        return SourceRootDirectory;
    }

    public bool IsAbsolutePath(string path) {
        return LibSassHost.FileManager.Instance.IsAbsolutePath(path);
    }
    public string ReadFile(string path) {
        string content;
        while (true) {
            try {
                content = File.ReadAllText(path);
                break;
            }
            catch (System.IO.IOException) {
                Console.WriteLine($"Waiting for {path}...");
                Thread.Sleep(100);
            }

        }
        var relativeDirectory = Path.GetRelativePath(SourceRootDirectory, Path.GetDirectoryName(path));
        relativeDirectory = relativeDirectory.Replace('\\', '/');
        var currentDirectoryVariable = $"$current-directory: \"{relativeDirectory}\";\n";
        return currentDirectoryVariable + content;
    }

    public string ToAbsolutePath(string path) {
        return null;
    }



    protected override string Update() {
        
        try {
            var options = new CompilationOptions { SourceMap = true };
            options.OutputStyle = OutputStyle.Compact;
            options.SourceMapFileUrls = true;
            //options.IncludePaths = new string[] { Path.Combine(Path.GetDirectoryName(InputFilePath), "Dependencies\\Csml\\Fonts") };


            SassCompiler.FileManager = this;// new FileManager(Path.GetDirectoryName(InputFilePath), LibSassHost.FileManager.Instance);
            CompilationResult result = SassCompiler.CompileFile(InitialFilePath, null, null, options);

            if (DeveloperMode) {
                observableFiles = CaptureModificationTimes(result.IncludedFilePaths);
            }

            OutputFileName = "style.css";
            var outputMapFileName = "style.css.map";
            if (!DeveloperMode) {
                var hash = Hash.CreateFromString(result.CompiledContent).ToString();
                OutputFileName = hash + ".css";
                outputMapFileName = OutputFileName + ".map";
            }
            

            string outputFilePath = Path.Combine(OutputRootDirectory, OutputFileName);
            string sourceMapFilePath = Path.Combine(OutputRootDirectory, outputMapFileName);

            File.WriteAllText(outputFilePath, result.CompiledContent);
            File.WriteAllText(sourceMapFilePath, result.SourceMap);

        }
        catch (SassСompilationException e) {
            if (e.File != null)
                if (!observableFiles.ContainsKey(e.File)) {
                    observableFiles.Add(e.File, File.GetLastWriteTime(e.File));
                }

            var files = observableFiles.Select(x => x.Key).ToArray();
            observableFiles = CaptureModificationTimes(files);

            if (!DeveloperMode) throw e;

            return e.Message;
        }
        return null;
    }
}
