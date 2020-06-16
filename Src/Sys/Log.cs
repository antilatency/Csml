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
        public static LogContext ToDo {
            get {
                return new LogContext {
                    logType = LogContext.LogType.todo
                };
            }
        }


    }

    public class LogContext {

        public static string prefix = "CSM";

        public enum LogType {
            info,
            todo,
            warning,
            error
        };


        public LogType logType;

        protected static ConsoleColor PrintTypeToColor(LogType printType) {
            if (printType == LogType.todo) return ConsoleColor.Green;
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
            Console.OutputEncoding = System.Text.Encoding.UTF8;
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
            string message, ErrorCode code = ErrorCode.Unassigned,
            //[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) {
            Print(sourceFilePath, sourceLineNumber, logType, message, code);
        }

        public void Unknown(
            string message, ErrorCode code = ErrorCode.Unassigned) {
            Print("<unknown>", 0, logType, message, code);
        }

        public void OnObject(
            object obj,
            string message, ErrorCode code = ErrorCode.Unassigned) {
            if (obj is IInfo) {
                IInfo callerInfo = obj as IInfo;
                Print(callerInfo.CallerSourceFilePath, callerInfo.CallerSourceLineNumber, logType, message, code);
            } else {
                Print("Unknown", 0, logType, message, code);
            }            
        }

        public void On(string path, int line, string message, ErrorCode code = ErrorCode.Unassigned) {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(2);
            Print(path, line, logType, message, code);
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

        }



    }
}
