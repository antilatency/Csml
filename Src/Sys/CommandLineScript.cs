using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Csml {
    public static class CommandLineScript {

        public static bool ExecuteCommandLineArguments<T>(string[] args) {
            return ExecuteCommandLineArguments(args, typeof(T));
        }
        enum State { Idle, Arg, QuotedArg };

        public static List<string> split(string cmdLine) {
            List<string> list = new List<string>();
            string arg = "";
            bool escape = false;

            State state = State.Idle;
            foreach (char c in cmdLine) {
                if(c == ',' && ((state != State.QuotedArg) || escape)) {
                    state = State.Idle;
                    list.Add(arg);
                    escape = false;
                    arg = "";
                    continue;
                }


                if (state == State.Idle) {
                    if (c == '~') {
                        state = State.QuotedArg;
                    } else if ((c != ' ')) {
                        arg = c.ToString();
                        state = State.Arg;
                    }
                } else if (state == State.QuotedArg) {
                    if (!escape) {
                        if(c == '~') {
                            escape = true;
                        } else {
                            arg += c;
                        }
                    } else {
                        if(c == '~') {
                            arg += c;
                            escape = false;
                        } else {
                            if(c == ' ') {
                                state = State.Idle;
                                escape = false;
                            } else {
                                throw new Exception("Ivalig args string format");
                            }
                        }
                    }
                } else if (state == State.Arg) {
                    if(c == ' ') {
                        state = State.Idle;
                    } else {
                        arg += c;
                    }
                }
            }



            if (!string.IsNullOrEmpty(arg)) list.Add(arg);
            return list;
        }

        class CliMethodDesc {
            public string Name;
            public List<string> Args;

            static bool IsParams(ParameterInfo param) {
                return param.IsDefined(typeof(ParamArrayAttribute), false);
            }

            static object ConvertParamValue(Type info, string value) {
                if(!info.IsValueType && (value == "null")) {
                    return null;
                }
                if(info == typeof(string)) return value;
                if (info == typeof(int)) return int.Parse(value);
                if (info == typeof(uint)) return uint.Parse(value);
                if (info == typeof(short)) return short.Parse(value);
                if (info == typeof(ushort)) return ushort.Parse(value);
                if (info == typeof(long)) return ulong.Parse(value);
                if (info == typeof(float)) return float.Parse(value);
                if (info == typeof(double)) return double.Parse(value);
                if (info == typeof(bool)) return bool.Parse(value);
                throw new Exception("type not supported");
            }

            private static object GetDefaultValue(Type type) {
                if (type.IsValueType) {
                    return Activator.CreateInstance(type);
                }
                return null;
            }

            public void Call(Type staticType) {
                List<object> argsValues = new List<object>();

                var m = staticType.GetMethod(Name);
                var p = m.GetParameters();

                int argId = 0;
                foreach(var param in p) {
                    if (!IsParams(param)) {
                        if (param.IsOptional && (argId >= Args.Count)) {
                            if (param.HasDefaultValue) {
                                argsValues.Add(param.DefaultValue);
                            } else {
                                argsValues.Add(param.ParameterType);
                            }
                        } else {
                            argsValues.Add(ConvertParamValue(param.ParameterType, Args[argId]));
                            argId++;
                        }
                    } else { 
                        var elementType = param.ParameterType.GetElementType();
                        var arraySize = Args.Count - argId;
                        var arrayOffset = 0;
                        Array arr = Array.CreateInstance(elementType, arraySize);

                        for(; arrayOffset < arraySize; ++arrayOffset) {
                            arr.SetValue(ConvertParamValue(elementType, Args[argId + arrayOffset]), arrayOffset);
                        }
                        argsValues.Add(arr);
                        break;
                    }
                }

                m.Invoke(null, argsValues.ToArray());

                Console.WriteLine("");
            }
        }

        private static CliMethodDesc ParseMethod(string cliMethodString) {
            var matches = Regex.Match(cliMethodString, @"([a-zA-Z][a-zA-Z0-9]*)\((.*)\);");

            if (matches.Success) {
                CliMethodDesc result = new CliMethodDesc();
                result.Name = matches.Groups[1].Value;
                result.Args = split(matches.Groups[2].Value);
                return result;
            } else {
                throw new Exception("WTF");
            }
        }

        public static bool ExecuteCommandLineArguments(string[] args, Type staticType = null) {
            var cliMethods = args.Select(v => ParseMethod(v));
            foreach(var method in cliMethods) {
                method.Call(staticType);
            }
            return true;
        }
    }
}
