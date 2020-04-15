using ColorCode;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Csml {
    public class Code : Code<Code> {
        private ProgrammingLanguage UserDefinedProgrammingLanguage;
        private ProgrammingLanguage ProgrammingLanguageBasedOnExtension;
        protected override ProgrammingLanguage GetProgrammingLanguage() {
            if (UserDefinedProgrammingLanguage != ProgrammingLanguage.Undefined) return UserDefinedProgrammingLanguage;
            if (ProgrammingLanguageBasedOnExtension != ProgrammingLanguage.Undefined) return ProgrammingLanguageBasedOnExtension;
            return ProgrammingLanguage.Undefined;
        }
        public static ProgrammingLanguage GetProgrammingLanguageByExtension(string extension) {
            extension = extension.TrimStart('.').ToLower();
            if (extension == "cs") return ProgrammingLanguage.CSharp;
            if (extension == "xml") return ProgrammingLanguage.Xml;
            if (extension == "json") return ProgrammingLanguage.JavaScript;
            throw new ArgumentException("Please add extension here");
        }
        protected Code() { }

        public static Code FromFile(string filePath, ProgrammingLanguage programmingLanguage = ProgrammingLanguage.Undefined, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "") {
            Code result = new Code();
            result.UserDefinedProgrammingLanguage = programmingLanguage;

            var absolutePath = ConvertPathToAbsolute(filePath, callerFilePath);
            if (!File.Exists(absolutePath)) {
                Log.Error.OnCaller($"File {absolutePath} not found");
            }
            result.ProgrammingLanguageBasedOnExtension = GetProgrammingLanguageByExtension(Path.GetExtension(filePath));
            result.Source = File.ReadAllText(absolutePath);
            return result;
        }

        public Code(string source, ProgrammingLanguage programmingLanguage = ProgrammingLanguage.Undefined) {
            UserDefinedProgrammingLanguage = programmingLanguage;
            Source = source;
        }

        protected override string GetFinalSourceCode() {
            return Source.ToString();
        }

        protected override Range? GetLineSpan() {
            return null;
        }
    }

    public enum ProgrammingLanguage {
        Undefined = 0,
        PowerShell,
        Haskell,
        Koka,
        FSharp,
        Typescript,
        Cpp,
        Css,
        Php,
        Xml,
        VbDotNet,
        Sql,
        Markdown,
        Fortran,
        Java,
        Html,
        CSharp,
        AspxVb,
        AspxCs,
        Aspx,
        Asax,
        Ashx,
        JavaScript
    }

    public abstract class Code<T> : Element<T> where T : Code<T> {

        ColorCode.ILanguage LanguageToColorCode(ProgrammingLanguage programmingLanguage) {
            var name = Enum.GetName(typeof(ProgrammingLanguage), programmingLanguage);
            var allColorCodeLanguages = typeof(ColorCode.Languages).GetProperties(BindingFlags.Static | BindingFlags.Public).Where(x => x.PropertyType == typeof(ColorCode.ILanguage));
            return (ILanguage)allColorCodeLanguages.FirstOrDefault(x => x.Name == name)?.GetValue(null);
        }


        protected object Source;
        protected Code() {
        }

        protected Code(T other) : this() {
            Source = other.Source;
        }

        protected abstract string GetFinalSourceCode();
        protected abstract Range? GetLineSpan();
        protected abstract ProgrammingLanguage GetProgrammingLanguage();


        public override IEnumerable<HtmlNode> Generate(Context context) {
            if (Source is GitHub.File) {
                var lineSpan = GetLineSpan();
                yield return HtmlNode.CreateNode("<a>").Do(x => {
                    x.AddClass("github-link");
                    x.SetAttributeValue("target", "_blank");
                    var href = (Source as GitHub.File).HtmlUri;
                    
                    if (lineSpan.HasValue) {
                        x.SetAttributeValue("href", $"{href}#L{lineSpan.Value.Start.Value + 1}-L{lineSpan.Value.End.Value + 1}");
                    } else {
                        x.SetAttributeValue("href", $"{href}");
                    }
                });


            }

            var code = GetFinalSourceCode();

            //https://github.com/WilliamABradley/ColorCode-Universal

            var formatter = new ColorCode.HtmlFormatter();

            var html = formatter.GetHtmlString(code, LanguageToColorCode(GetProgrammingLanguage()));
            yield return HtmlNode.CreateNode(html).Do(x => {
                x.Attributes.RemoveAll();
                x.AddClass("code");
            });
        }

        public static string SpacesOrTabsOnly(string x) {
            string result = "";
            for (int i = 0; i < x.Length; i++) {
                if ((x[i] == ' ') | (x[i] == '\t')) {
                    result += x[i];
                }
            }
            return result;
        }
        public static string Untab(string code, string tabs) {
            if (string.IsNullOrEmpty(tabs)) return code;

            StringBuilder stringBuilder = new StringBuilder();
            using (StringReader reader = new StringReader(code)) {
                string line = string.Empty;
                do {
                    line = reader.ReadLine();
                    if (line != null) {
                        if (line.StartsWith(tabs))
                            line = line.Substring(tabs.Length);
                        stringBuilder.AppendLine(line);
                    }

                } while (line != null);
            }
            return stringBuilder.ToString();
        }
    }

}