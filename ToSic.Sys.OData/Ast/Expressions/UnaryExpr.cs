namespace ToSic.Sys.OData.Ast;

public sealed class UnaryExpr(string op, Expr operand) : Expr
{
    public const string Not = "not";

    /// <summary>
    /// The operation, as of now only "not" are possible.
    /// </summary>
    /// <remarks>
    /// Case sensitivity must be ensured when creating this object
    /// as we always only check for "not" in lower case. 
    /// </remarks>
    public string Op { get; } = op;

    public Expr Operand { get; } = operand;

    public override string ToString() => $"({Op} {Operand})";
}