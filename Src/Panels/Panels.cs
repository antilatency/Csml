using System;

namespace Csml {
    public class Panel<T> : Container<T> where T:Panel<T> {
        public Panel(FormattableString content) : base("div", "Panel", typeof(T).Name) {
            if (content != null) Add(content);
        }
    }

    public class Info : Panel<Info> {
        public Info(FormattableString content = null) : base(content) { }
    }
    public class Error : Panel<Error> {
        public Error(FormattableString content = null) : base(content) { }
    }
    public class Bug : Panel<Bug> {
        public Bug(FormattableString content = null) : base(content) { }
    }
    public class Note : Panel<Note> {
        public Note(FormattableString content = null) : base(content) { }
    }
    public class Success : Panel<Success> {
        public Success(FormattableString content = null) : base(content) { }
    }
    public class Warning : Panel<Warning> {
        public Warning(FormattableString content = null) : base(content) { }
    }

}