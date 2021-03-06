using System;
using System.Collections.Generic;
using System.IO;

namespace Csml {
    public abstract class FileProcessor {

        public readonly string SourceRootDirectory;
        public readonly string OutputRootDirectory;
        protected bool DeveloperMode;
        public string Error { get; protected set; }
        public bool Success => Error == null;

        protected Dictionary<string, DateTime> observableFiles = new Dictionary<string, DateTime>();

        public FileProcessor(bool developerMode, string sourceRootDirectory, string outputRootDirectory) {
            DeveloperMode = developerMode;
            SourceRootDirectory = sourceRootDirectory;
            OutputRootDirectory = outputRootDirectory;
        }

        public virtual bool IsChanged() {
            if (observableFiles == null) return false;
            foreach (var i in observableFiles) {
                if (!File.Exists(i.Key)) return true;
                DateTime modification = File.GetLastWriteTime(i.Key);
                if (modification > i.Value) return true;
            }
            return false;
        }

        public bool UpdateIfChanged() {
            var isChanged = IsChanged();
            if (isChanged) {
                Error = Update();
            }
            return isChanged;
        }

        protected static Dictionary<string, DateTime> CaptureModificationTimes(IEnumerable<string> filePaths) {
            var result = new Dictionary<string, DateTime>();
            foreach (var i in filePaths) {
                DateTime modification = File.GetLastWriteTime(i);
                result.Add(i, modification);
            }
            return result;
        }

        protected abstract string Update();
    }
}
