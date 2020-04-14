
using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class SassProcessor : FileProcessor, IFileManager {
    
    private string InitialFilePath;    

    public SassProcessor(string sourceRootDirectory, string outputRootDirectory, string initialFilePath):
        base(sourceRootDirectory, outputRootDirectory) {
        
        if (Path.IsPathRooted(initialFilePath)) {
            InitialFilePath = initialFilePath;
        } else {
            InitialFilePath = Path.Combine(SourceRootDirectory, initialFilePath);
        }
        
        InitializeWriteTimes();
        Error = Update();
    } 

    void InitializeWriteTimes() {
        observableFiles = new Dictionary<string, DateTime>();
        observableFiles.Add(InitialFilePath, new DateTime());
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
        string content = "";
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
        string outputFilePath = Path.Combine(OutputRootDirectory, "style.css");
        string sourceMapFilePath = Path.Combine(OutputRootDirectory, "style.css.map");
        try {
            var options = new CompilationOptions { SourceMap = true };
            options.OutputStyle = OutputStyle.Compact;
            options.SourceMapFileUrls = true;
            //options.IncludePaths = new string[] { Path.Combine(Path.GetDirectoryName(InputFilePath), "Dependencies\\Csml\\Fonts") };


            SassCompiler.FileManager = this;// new FileManager(Path.GetDirectoryName(InputFilePath), LibSassHost.FileManager.Instance);
            CompilationResult result = SassCompiler.CompileFile(InitialFilePath, outputFilePath,
                sourceMapFilePath, options);

            observableFiles = CaptureModificationTimes(result.IncludedFilePaths);

            File.WriteAllText(outputFilePath, result.CompiledContent);
            File.WriteAllText(sourceMapFilePath, result.SourceMap);

        }
        catch (SassСompilationException e) {
            if (e.File != null)
                if (!observableFiles.ContainsKey(e.File)) {
                    observableFiles.Add(e.File, File.GetLastWriteTime(e.File));
                } else {
                    observableFiles[e.File] = File.GetLastWriteTime(e.File);
                }

            return e.Message;
        }
        return null;
    }
}
