using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Csml {
    
    public partial class Language {
        public string name;
        public Language(string name) {
            this.name = name;
        }

        public static Language NameToLanguage(string name) {
            var fields = typeof(Language).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(x=>x.FieldType == typeof(Language));
            foreach (var f in fields) {
                Language value = (Language)f.GetValue(null);
                if (name.EndsWith("_" + value.name))
                    return value;
            }
            return null;
        }
    }

    public class Translatable : Translatable<Translatable> {
        
        /*public Translatable([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0){
            SetCallerInfo(memberName, sourceFilePath, sourceLineNumber);
        }*/
    }

    public interface ITranslatable {
        public string PrimaryName { get; set; }
        public Language Language { get; set; }
        public ITranslatable GetTranslation(Language language);
        public void AddTranslation(ITranslatable translatable);
    }

    public class Translatable<T> : Collection<T>, ITranslatable where T : Translatable<T> {
        public string PrimaryName { get; set; }
        public Language Language { get; set; }
        private Dictionary<Language, T> Translations { get; } = new Dictionary<Language, T>();
        
        public ITranslatable GetTranslation(Language language) {
            return Translations[language] as ITranslatable;
        }
        public void AddTranslation(ITranslatable translatable) {
            Translations.Add(translatable.Language, translatable as T);
        }        

        public T SetLanguage(Language language) {
            Language = language;
            return this as T;
        }
        public T this[Language language] {
            get {
                return SetLanguage(language);
            }
        }

        /*public T SetLanguage(Language language) {
            this.Language = language;
            return this as T;
        }*/

        public static void AssignLanguages<R>() {
            var fields = typeof(R).GetFields(BindingFlags.Static | BindingFlags.Public);

            HashSet<string> names = new HashSet<string>();

            foreach (var f in fields) {

                var value = f.GetValue(null);

                if (value is ITranslatable) {
                    var translatable = value as ITranslatable;
                    if (translatable.Language == null) {
                        translatable.Language = Language.NameToLanguage(f.Name);
                        if (translatable.Language == null) {
                            Log.Error.OnObject(value, "Language not defined.", ErrorCode.LanguageNotDefined);                         
                        }
                    }

                    string suffix = "_" + translatable.Language.name;
                    if (f.Name.EndsWith(suffix)) {
                        translatable.PrimaryName = f.Name.Substring(0, f.Name.Length - suffix.Length);
                    } else {
                        translatable.PrimaryName = f.Name;
                    }
                    names.Add(translatable.PrimaryName);
                }
            }



            foreach (var n in names) {
                var translations = fields.Select(x => x.GetValue(null))
                    .Where(x => x is ITranslatable).Select(x => x as ITranslatable)
                    .Where(x => x.PrimaryName == n);

                foreach (var t0 in translations) {
                    foreach (var t1 in translations) {
                        if (t0 != t1) {
                            var t0Final = (t0 as IFinal).ImplementerType;
                            var t1Final = (t1 as IFinal).ImplementerType;

                            if (t0Final != t1Final) {
                                Log.Warning.OnObject(t0, $"Wrong translation type: {t0Final} != {t1Final}", ErrorCode.WrongTranslationType);
                                Log.Error.OnObject(t1, $"Wrong translation type: {t1Final} != {t0Final}", ErrorCode.WrongTranslationType);
                            }
                            t0.AddTranslation(t1);
                        }
                    }
                }

            }

        }
    }
}