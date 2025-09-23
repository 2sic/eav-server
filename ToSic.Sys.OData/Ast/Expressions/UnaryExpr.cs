namespace ToSic.Sys.OData.Ast;

public sealed class UnaryExpr : Expr
{
    public string Op { get; }
    public Expr Operand { get; }
    public UnaryExpr(string op, Expr operand) { Op = op; Operand = operand; }
    public override string ToString() => $"({Op} {Operand})";
}