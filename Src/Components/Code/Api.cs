using Csml;
public static class ApiStatic {
    public static IElement Keyword(string x) => new Modify(x.ToFormattableString()).AddClasses("keyword");
    public static IElement Interface => Keyword("interface");
    public static IElement Struct => Keyword("struct");
    public static IElement Enum => Keyword("enum");

    public static IElement Namespace(string name) => new Modify(name.ToFormattableString()).AddClasses("Namespace");
    public static IElement Type(string name) => new Modify(name.ToFormattableString()).AddClasses("Type");
    public static IElement Method(IElement reference) => new Modify(reference).AddClasses("Method");
    public static IElement CodeBlock(IElement reference) => new Modify(reference).Wrap("pre").Wrap("div").AddClasses("code");
    public static IElement CodeInline(IElement reference) => new Modify(reference).Wrap("code");
}