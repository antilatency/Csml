using System;
using System.Drawing;
using System.IO;
using System.Threading;

namespace Csml {
    public static class Utils {

        public static string ReadAllText(string path, int timeoutMs = 5000) {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while(stopwatch.ElapsedMilliseconds < timeoutMs) {
                try {
                    return File.ReadAllText(path);
                }
                catch (System.IO.IOException) { 
                    
                }
                Thread.Sleep(10);
            }
            Log.Error.OnCaller($"Failed to read file: {path}");

            return "";//will never happen
        }

        public static void CreateDirectory(string directory, int timeoutMs = 2000) {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var last = Path.GetDirectoryName(directory);
            if (!Directory.Exists(last)) CreateDirectory(last);
            DirectoryInfo directoryInfo = Directory.CreateDirectory(directory);            
            while (!directoryInfo.Exists) {
                if (stopwatch.ElapsedMilliseconds > timeoutMs)
                    Log.Error.OnCaller($"Failed to create directory: {directory}");
                Thread.Sleep(10);
            }
            
        }


        public static void DeleteDirectory(string directory, int timeoutMs = 2000) {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            if (Directory.Exists(directory)) {
                Directory.Delete(directory, true);
                while (Directory.Exists(directory)) {
                    if (stopwatch.ElapsedMilliseconds > timeoutMs)
                        Log.Error.OnCaller($"Failed to delete directory: {directory}");
                    Thread.Sleep(10);
                }
            }
                
        }

        public static void DeleteEmptySubdirectories(string directory) {
            foreach (var d in Directory.GetDirectories(directory)) {
                DeleteEmptySubdirectories(d);
                if (Directory.GetFiles(d).Length == 0 &&
                    Directory.GetDirectories(d).Length == 0) {
                    Directory.Delete(d, false);
                }
            }            
        }
        

        public static Exception GetDeepestException(Exception e) {
            if (e.InnerException == null) {
                return e;
            } else {
                return GetDeepestException(e.InnerException);
            }
        }

        public static string ThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            return sourceFilePath;
        }
        public static string ThisDirectory([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            return Path.GetDirectoryName(sourceFilePath);
        }

        public static Color Lerp(this Color s, Color t, float k) {
            var bk = (1 - k);
            var a = s.A * bk + t.A * k;
            var r = s.R * bk + t.R * k;
            var g = s.G * bk + t.G * k;
            var b = s.B * bk + t.B * k;
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        public static uint ToRgba(this Color color) {
            var argb = (uint)color.ToArgb();
            uint rgba = argb << 8;
            rgba |= argb >> 24;
            return rgba;
        }

        public static string ToRgbaString(this Color color) {
            return color.ToRgba().ToString("X8");
        }

        public static uint ToRgb(this Color color) {
            var argb = (uint)color.ToArgb();
            uint rgba = argb & 0xFFFFFF;
            return rgba;
        }
        public static string ToRgbString(this Color color) {
            return color.ToRgb().ToString("X6");
        }

        public static FormattableString ToFormattableString(this string x) {
            return $"{x}";
        }

        public static Text ToText(this FormattableString x) {
            return new Text(x);
        }
        public static Paragraph ToParagraph(this FormattableString x) {
            return new Paragraph(x);
        }


    }


}