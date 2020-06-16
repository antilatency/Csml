using System;

namespace Csml {
    class Stopwatch : IDisposable {

        private System.Diagnostics.Stopwatch stopwatch;
        private string SourceFilePath;
        private int SourceLineNumber;
        private string Message;
        public Stopwatch(string message,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;
            Message = message;
        }

        public void Dispose() {
            Log.Info.On(SourceFilePath, SourceLineNumber, Message+" "+stopwatch.ElapsedMilliseconds+"ms");
        }

    }
}