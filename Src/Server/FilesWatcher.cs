using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csml.Server {
    public static class FilesWatcher
    {

        private static bool Running = false;
        private static Thread WatchThread;
        private static object Sync = new object();

        public static int UpdateId { get; private set; }

        private static void Watch() {
            bool reloadRequired = true;
            string ScssError = null;

            while (Running) {
                if (reloadRequired)
                {
                    UpdateId += 1;
                    Console.WriteLine("### Files reload: " + UpdateId);
                    reloadRequired = false;
                }
                if (CsmlApplication.SassProcessor.Error != ScssError) {
                    ScssError = CsmlApplication.SassProcessor.Error;
                    Console.Clear();
                    if (ScssError != null) {
                        Console.WriteLine("Scss:" + ScssError);
                    }
                }

                System.Threading.Thread.Sleep(250);

                reloadRequired |= CsmlApplication.SassProcessor.UpdateIfChanged();
                reloadRequired |= CsmlApplication.JavascriptProcessor.UpdateIfChanged();
            }
        }

        public static void Start() {
            lock (Sync) {
                if (WatchThread == null) {
                    Running = true;
                    Console.WriteLine("### Start files watching");
                    WatchThread = new Thread(Watch);
                    WatchThread.Start();
                }
            }
        }

        public static void Stop() {
            lock (Sync) {
                if (WatchThread != null) {
                    Console.WriteLine("### Stop files watching");
                    Running = false;
                    WatchThread.Join();
                    WatchThread = null;
                }
            }
        }
    }
}
