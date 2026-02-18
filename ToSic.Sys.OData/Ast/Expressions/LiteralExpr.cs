namespace ToSic.Sys.OData.Ast;

public sealed class LiteralExpr(object? value) : Expr
{
    public object? Value { get; } = value;

    public override string ToString() =>
        Value switch
        {
            null => "null",
            string s => $"'{s}'",
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => Convert.ToString(Value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
        };
}