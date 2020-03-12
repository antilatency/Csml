namespace Csml {

    public static class Preprocessor {

        public static void Process<R>() {

            Translatable.AssignLanguages<R>();

            Term.SetTermNames<R>();

            CheckForInstanceFields<R>();
        }

        static void CheckForInstanceFields<T>(){
            //typeof(T).GetFields()
        }
    }
}