using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Csml {
    public static class ScopeHelper {

        public static readonly BindingFlags PropertyBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static readonly BindingFlags GetOncePropertyBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;


        public static readonly IEnumerable<Type> AllStatic;
        public static readonly IEnumerable<Scope> All;

        static ScopeHelper() {

            var b = typeof(Scope).IsSubclassOf(typeof(Scope));

            AllStatic = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(Scope)))
                    .Where(x => !x.IsGenericType);

            All = AllStatic.Select(x => (Scope)Activator.CreateInstance(x));
        }

        public static IEnumerable<T> GetScopePropertiesOfType<ScopeType, T>() {
            var result = typeof(ScopeType).GetProperties(PropertyBindingFlags)
                .Where(x => typeof(T).IsAssignableFrom(x.PropertyType))
                .Select(x => (T)x.GetValue(null));
            return result;
        }

        public static void EnableGetOnce() {
            AllStatic.Append(typeof(Scope)).ForEach(x => EnableGetOnce(x));
        }

        public static void EnableGetOnce(Type t) {
            var nonStaticMembers = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (nonStaticMembers.Count() > 0) {
                Log.Error.Here($"Scope {t.Name} contains non static prooperty {nonStaticMembers.First().Name}");
            }
            foreach (var p in t.GetProperties(GetOncePropertyBindingFlags)) {
                GetOnce.WrapPropertyGetter(p);
            }
        }
    }
}
