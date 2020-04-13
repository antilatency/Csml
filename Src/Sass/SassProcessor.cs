
using LibSassHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class SassProcessor : IFileManager{
    public string Error { get; private set; }
    public bool Success => Error == null;

    private string SourceRootDirectory;
    private string OutputRootDirectory;
    private string InitialFilePath;
    private Dictionary<string, DateTime> includedFilesWriteTime = new Dictionary<string, DateTime>();

    public SassProcessor(string sourceRootDirectory, string outputRootDirectory, string initialFilePath) {
        SourceRootDirectory = sourceRootDirectory;
        OutputRootDirectory = outputRootDirectory;
        if (Path.IsPathRooted(initialFilePath)) {
            InitialFilePath = initialFilePath;
        } else {
            InitialFilePath = Path.Combine(SourceRootDirectory, initialFilePath);
        }
        
        InitializeWriteTimes();
        Error = Compile();

    }
    public bool IsChanged() {
        if (includedFilesWriteTime == null) return false;
        foreach (var i in includedFilesWriteTime) {
            if (!File.Exists(i.Key)) return true;
            DateTime modification = File.GetLastWriteTime(i.Key);
            if (modification > i.Value) return true;
        }
        return false;
    }

    public bool UpdateIfChanged() {
        var isChanged = IsChanged();
        if (IsChanged()) {
            Error = Compile();
        }
        return isChanged;
    }

    void InitializeWriteTimes() {
        includedFilesWriteTime = new Dictionary<string, DateTime>();
        includedFilesWriteTime.Add(InitialFilePath, new DateTime());
    }

    private Dictionary<string, DateTime> CaptureModificationTimes(IEnumerable<string> filePaths) {
        var result = new Dictionary<string, DateTime>();
        foreach (var i in filePaths) {
            DateTime modification = File.GetLastWriteTime(i);
            result.Add(i, modification);
        }
        return result;
    }


    //
    // Summary:
    //     Defines a interface of file manager
    /*public class FileManager : IFileManager {
        private IFileManager DefaultFileManager;

        public string SourceRootDirectory { get; set; }

        public FileManager(string sourceRootDirectory, IFileManager defaultFileManager) {
            SourceRootDirectory = sourceRootDirectory;
            DefaultFileManager = defaultFileManager;
        }*/

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

            //var content = DefaultFileManager.ReadFile(path);
            var relativeDirectory = Path.GetRelativePath(SourceRootDirectory, Path.GetDirectoryName(path));
            //var relativeDirectory = Path.GetDirectoryName(relativePath);
            relativeDirectory = relativeDirectory.Replace('\\', '/');
            var currentDirectoryVariable = $"$current-directory: \"{relativeDirectory}\";\n";

            //$currentDirectory: "Fonts/roboto"


            return currentDirectoryVariable + content;


        }

        public string ToAbsolutePath(string path) {
            return null;
        }
    //}


    private string Compile() {
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

            includedFilesWriteTime = CaptureModificationTimes(result.IncludedFilePaths);

            File.WriteAllText(outputFilePath, result.CompiledContent);
            File.WriteAllText(sourceMapFilePath, result.SourceMap);

        }
        catch (SassСompilationException e) {
            if (e.File != null)
                if (!includedFilesWriteTime.ContainsKey(e.File)) {
                    includedFilesWriteTime.Add(e.File, File.GetLastWriteTime(e.File));
                } else {
                    includedFilesWriteTime[e.File] = File.GetLastWriteTime(e.File);
                }

            return e.Message;
        }
        return null;
    }
}
