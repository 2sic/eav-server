namespace ToSic.Sys.OData.Ast;

public sealed class ListExpr(IEnumerable<Expr> items) : Expr
{
    public List<Expr> Items { get; } = [..items ?? []];
    public override string ToString() => $"({string.Join(", ", Items)})";
}