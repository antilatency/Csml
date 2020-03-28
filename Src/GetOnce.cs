using PostSharp.Aspects;
using PostSharp.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace Csml {

    [PSerializable]
    public class GetOnceAttribute : LocationInterceptionAspect {

        object Backup;
        public override void OnGetValue(LocationInterceptionArgs args) {
            if (Backup != null) {
                args.Value = Backup;
                return;
            }

            args.ProceedGetValue();
            Backup = args.Value;

            if (args.Value is IPropertyInitializer) {
                
                (args.Value as IPropertyInitializer).AfterInitialization(args.Instance, args.LocationName, args.Location.PropertyInfo);
            }
        }

        public override void OnSetValue(LocationInterceptionArgs args) {
            base.OnSetValue(args);
        }

        public override void OnInstanceLocationInitialized(LocationInitializationArgs args) {
            base.OnInstanceLocationInitialized(args);
        }

    }

    public interface IPropertyInitializer {
        void AfterInitialization(object parent, string propertyName, PropertyInfo propertyInfo);
    }


    public class Property {
        public object Parent;
        public string PropertyName;
        public PropertyInfo PropertyInfo;
        public string NameWithoutLanguage {
            get {
                foreach (var l in Language.All) {
                    var languageSuffix = "_" + l.Name;
                    if (PropertyName.EndsWith(languageSuffix))
                        return PropertyName.Substring(0, PropertyName.Length - languageSuffix.Length);
                }
                return PropertyName;
            }
        }
        public Language Language {
            get {
                foreach (var l in Language.All) {
                    var languageSuffix = "_" + l.Name;
                    if (PropertyName.EndsWith(languageSuffix))
                        return l;
                }
                return null;
            }
        }
        /*public List<object> Translations {
            get {
                if (Language == null) return null;
                var properties = PropertyInfo.DeclaringType.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|bf)
            }
        }*/
    }

    /*public class PropertyInitializer : IPropertyInitializer {
        protected Property Property;

        
    }*/



}

