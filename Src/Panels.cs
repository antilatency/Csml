using System;

namespace Csml {
    public class Panel<T> : Container<T> where T:Panel<T> {
        public Panel(FormattableString content, string name) : base("div", "panel", name) {
            if (content != null) Add(content);
        }
    }

    public class Info : Panel<Info> {
        public Info(FormattableString content = null) : base(content, "info") { }
    }
    public class Error : Panel<Error> {
        public Error(FormattableString content = null) : base(content, "error") { }
    }
    public class Bug : Panel<Bug> {
        public Bug(FormattableString content = null) : base(content, "bug") { }
    }
    public class Note : Panel<Note> {
        public Note(FormattableString content = null) : base(content, "note") { }
    }
    public class Success : Panel<Success> {
        public Success(FormattableString content = null) : base(content, "success") { }
    }
    public class Warning : Panel<Warning> {
        public Warning(FormattableString content = null) : base(content,"warning") { }
    }

}