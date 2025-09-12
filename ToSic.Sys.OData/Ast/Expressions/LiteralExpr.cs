namespace ToSic.Sys.OData.Ast;

public sealed class LiteralExpr : Expr
{
    public object Value { get; }
    public LiteralExpr(object value) { Value = value; }
    public override string ToString()
    {
        if (Value is string s) return $"'{s}'";
        if (Value is IFormattable f) return f.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
        return Convert.ToString(Value, System.Globalization.CultureInfo.InvariantCulture);
    }
}