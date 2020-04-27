using Csml;
public sealed partial class Api : Scope {
    public static IElement Keyword(string x) => new Modify(x.ToFormattableString()).AddClasses("keyword");
    public static IElement Interface => Keyword("interface");
    public static IElement Type(IElement reference) => new Modify(reference).AddClasses("type");
    public static IElement Type(string name) => new Modify(name.ToFormattableString()).AddClasses("type");
    public static IElement Method(IElement reference) => new Modify(reference).AddClasses("method");
    public static IElement CodeBlock(IElement reference) => new Modify(reference).Wrap("pre").Wrap("div").AddClasses("code");
}