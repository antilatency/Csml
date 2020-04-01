using System;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Csml {

    public enum ErrorCode {
        Unassigned = 0,
        LanguageNotDefined = 1,
        WrongTranslationType = 2,

        StaticNotAllowed = 3
    }


    public class Log {

        public static LogContext Error {
            get {
                return new LogContext {
                    logType = LogContext.LogType.error
                };
            }
        }
        public static LogContext Warning {
            get {
                return new LogContext {
                    logType = LogContext.LogType.warning
                };
            }
        }
        public static LogContext Info {
            get {
                return new LogContext {
                    logType = LogContext.LogType.info
                };
            }
        }



    }

    public class LogContext {

        public static string prefix = "CSM";

        public enum LogType {
            info,
            warning,
            error
        };


        public LogType logType;

        protected static ConsoleColor PrintTypeToColor(LogType printType) {
            if (printType == LogType.info) return ConsoleColor.Gray;
            if (printType == LogType.warning) return ConsoleColor.Yellow;
            if (printType == LogType.error) return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        protected static string ErrorCodeToNumber(ErrorCode code) {
            return ((uint)code).ToString("0000").Substring(0, 4);
        }

        protected static void Print(string sourceFilePath, int sourceLineNumber, LogType type, string message, ErrorCode code) {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = PrintTypeToColor(type);

            string codeString = "";
            if (type != LogType.info) codeString = prefix + ErrorCodeToNumber(code) + ": ";

            string fullMessage = $"{sourceFilePath}({sourceLineNumber}): {type} {codeString }{message}";

            Console.WriteLine(fullMessage);
            Console.ForegroundColor = color;

            if (type == LogType.error) {
                //Console.ReadLine();
                throw new Exception(fullMessage);
            }
        }

        public void Here(
            string message, ErrorCode code,
            //[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
            Print(sourceFilePath, sourceLineNumber, logType, message, code);
        }

        public void OnObject(
            object obj,
            string message, ErrorCode code) {
            if (obj is IInfo) {
                IInfo callerInfo = obj as IInfo;
                Print(callerInfo.CallerSourceFilePath, callerInfo.CallerSourceLineNumber, logType, message, code);
            } else {
                Print("Unknown", 0, logType, message, code);
            }            
        }

        public void OnCaller(string message, ErrorCode code = ErrorCode.Unassigned) {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(2);
            Print(frame.GetFileName(), frame.GetFileLineNumber(), logType, message, code);
        }

        public void OnException(Exception e) {
            Regex regex = new Regex("at (.*) in (.*):line (\\d*)");
            var match = regex.Match(e.StackTrace);
            if (match != null) {
                Print(match.Groups[2].Value, int.Parse(match.Groups[3].Value), logType, e.Message, 0);                
            } else {
                Print("Unknown", 0, logType, e.Message, 0);
            }

            /*if (obj is ICallerInfo) {
                ICallerInfo callerInfo = obj as ICallerInfo;
                Print(callerInfo.callerSourceFilePath, callerInfo.callerSourceLineNumber, logType, message, code);
            } else {
                Print("Unknown", 0, logType, message, code);
            }*/
        }


        /*protected static string GetTypeSourceFilePath(ICustomAttributeProvider type) {
            var attributes = (SourceCodeMarkerAttribute[])type.GetCustomAttributes(typeof(SourceCodeMarkerAttribute), true);
            if (attributes.Length > 0)
                return attributes[0].sourceFilePath;
            return "";
        }
        protected static int GetTypeSourceLineNumber(ICustomAttributeProvider type) {
            var attributes = (SourceCodeMarkerAttribute[])type.GetCustomAttributes(typeof(SourceCodeMarkerAttribute), true);
            if (attributes.Length > 0)
                return attributes[0].sourceLineNumber;
            return 0;
        }

        protected static void StackTraceToPublicApi(out string sourceFilePath, out int sourceLineNumber) {
            StackTrace stackTrace = new StackTrace(true);
            var generatorAssembly = Assembly.GetAssembly(typeof(AntilatencyApiGenerator));

            foreach (var f in stackTrace.GetFrames()) {
                if (Assembly.GetAssembly(f.GetMethod().DeclaringType) != generatorAssembly) {
                    sourceFilePath = f.GetFileName();
                    sourceLineNumber = f.GetFileLineNumber();
                    return;
                }
            }
            sourceFilePath = "";
            sourceLineNumber = 0;
        }*/

    }
}

/*
public class Error : Log {
    
    public static void PublicApiCall(string message, ErrorCode code) {
        string sourceFilePath;
        int sourceLineNumber;
        StackTraceToPublicApi(out sourceFilePath, out sourceLineNumber);
        Print(sourceFilePath, sourceLineNumber, PrintType.error, message, code);
    }
    public static void Idlcs(ICustomAttributeProvider item, string message, ErrorCode code,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
        Print(GetTypeSourceFilePath(item), GetTypeSourceLineNumber(item), PrintType.error, message, code);
    }



}

public class Warning : Log {

    public static void Here(
        string message, ErrorCode code,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
        Print(sourceFilePath, sourceLineNumber, PrintType.warning, message, code);
    }

    public static void Idlcs(ICustomAttributeProvider item, string message, ErrorCode code,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
        Print(GetTypeSourceFilePath(item), GetTypeSourceLineNumber(item), PrintType.warning, message, code);
    }
}

public class Info : Log {
    public static void Here(
        string message,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
        Print(sourceFilePath, sourceLineNumber, PrintType.info, message, (ErrorCode)0);
    }

}*/