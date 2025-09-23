namespace ToSic.Sys.OData.Internal;

internal readonly struct Token
{
    public TokKind Kind { get; }
    public string Text { get; }
    public Token(TokKind kind, string text) { Kind = kind; Text = text; }
    public override string ToString() => Text ?? Kind.ToString();
}