using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Csml {
    public static class CommonExtensions {

        public static T Single<T>(this IEnumerable<T> source) {
            if (source.Count() == 1) {
                return source.First();
            }

            throw new ArgumentException("Use of Single assumes that collection contains only one element.");
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
            }
        }

        public static IEnumerable<T> Visit<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var item in source) {
                action(item);
                yield return item;
            }
        }

        public static PropertyInfo[] GetPropertiesAll(this Type x) {
            return x.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        public static bool ImplementsInterface(this Type x, Type interfaceType) {
            return x.GetInterfaces().Contains(interfaceType);
        }
    }
}
