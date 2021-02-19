using System.IO;
using System;
using Htmlilka;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Csml {

    public struct Context /*: IEquatable<Context>*/ {

        public float EstimatedWidth;
        public bool AForbidden;
        public string FormatString;
        public Language Language { get; set; }
        public IMaterial CurrentMaterial { get; set; }


        /*public override int GetHashCode() {
            return
                AForbidden.GetHashCode()
                ^ FormatString.GetHashCode()
                ^ Language.GetHashCode();
        }

        public bool Equals([AllowNull] Context other) {
            throw new NotImplementedException();
        }

        public static bool operator == (Context a, Context b) {
            return a.Equals(b);
        }

        public static bool operator !=(Context a, Context b) {
            return !(a == b);
        }*/

        /*public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var b2 = (BOX)obj;
            return (length == b2.length && breadth == b2.breadth && height == b2.height);
        }*/

        

    }

}