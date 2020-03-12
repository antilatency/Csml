using System;
using System.Collections.Generic;

namespace Csml {

    public class Collection<T>: CallerInfo<T> where T : class {
        public List<object> elements = new List<object>();

        public T Add(object element) {
            elements.Add(element);
            return this as T;
        }
        public T Add(FormattableString element) {
            elements.Add(element);
            return this as T;
        }


        public static object SimplifyFormattableString(FormattableString formattableString) {
            if (formattableString.Format == "{0}") return formattableString.GetArgument(0);
            if (formattableString.ArgumentCount == 0) return formattableString.Format;
            return formattableString;
        }

        public T this[FormattableString element] { get => Add(element); }
        public T this[object element] { get => Add(element); }

        public T this[Action<T> lambda] {
            get {
                lambda(this as T);
                return this as T;
            }
        }


    }
}