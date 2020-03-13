using System;
using System.Reflection;

namespace Csml {
    public sealed class Term : Term<Term> {
        public Term(object titleImage, FormattableString description) : base(titleImage, description) { 

        }
    }

    public interface ITerm { 
        public string Title { get; set; }
    }

    public class Term<T> :  Material<T>, ITerm where T : Term<T> {
        public Term(object titleImage, FormattableString description) : base(null,titleImage, description) {}

        public static void SetTermNames<R>(){
            var fields = typeof(R).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var f in fields) {
                var value = f.GetValue(null) ;
                if (value is ITerm) {
                    (value as ITerm).Title = (value as ITranslatable).PrimaryName.Replace("_"," ");
                }
            }
        }
    }
}