using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csml.Server {
    public static class FilesWatcher {
        private static bool Running = false;
        private static Thread WatchThread;
        private static object Sync = new object();

        public static string SassHash { get; private set; }
        public static string JavascriptHash { get; private set; }

        private static void UpdateHashes(bool force) {
            if (CsmlApplication.SassProcessor.UpdateIfChanged() || force) {
                var path = Path.Combine(CsmlApplication.SassProcessor.OutputRootDirectory, CsmlApplication.SassProcessor.OutputFileName);
                SassHash = Hash.CreateFromString(File.ReadAllText(path)).ToString();
            }

            if (CsmlApplication.JavascriptProcessor.UpdateIfChanged() || force) {
                var path = Path.Combine(CsmlApplication.JavascriptProcessor.OutputRootDirectory, CsmlApplication.JavascriptProcessor.OutputFileName);
                JavascriptHash = Hash.CreateFromString(File.ReadAllText(path)).ToString();
            }
        }

        private static void Watch() {
            string ScssError = null;
            while (Running) {
                if (CsmlApplication.SassProcessor.Error != ScssError) {
                    ScssError = CsmlApplication.SassProcessor.Error;
                    Console.Clear();
                    if (ScssError != null) {
                        Console.WriteLine("Scss:" + ScssError);
                    }
                }
                UpdateHashes(false);
                System.Threading.Thread.Sleep(250);
            }
        }

        public static void Start() {
            lock (Sync) {
                if (WatchThread == null) {
                    UpdateHashes(true);
                    Running = true;
                    WatchThread = new Thread(Watch);
                    WatchThread.Start();
                }
            }
        }

        public static void Stop() {
            lock (Sync) {
                if (WatchThread != null) {
                    Running = false;
                    WatchThread.Join();
                    WatchThread = null;
                }
            }
        }
    }
}
