using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csml {

    public partial class Language {
        public string Name;
        public string FullName;
        public Language(string name, string fullName) {
            Name = name;
            FullName = fullName;
        }

        public static Language NameToLanguage(string name) {
            var fields = typeof(Language).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.FieldType == typeof(Language));
            foreach (var f in fields) {
                Language value = (Language)f.GetValue(null);
                if (name.EndsWith("_" + value.Name))
                    return value;
            }
            return null;
        }

        public static List<Language> All => typeof(Language)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(Language)).Select(x => (Language)x.GetValue(null)).ToList();


        public override string ToString() {
            return Name;
        }


    }
}