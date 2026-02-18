namespace ToSic.Sys.OData.Internal;

internal readonly struct Token(TokKind kind, string? text)
{
    public TokKind Kind { get; } = kind;
    public string Text { get; } = text ?? string.Empty;
    public override string ToString() => Text;
}