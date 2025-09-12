namespace ToSic.Sys.OData.Ast;

public sealed class ListExpr : Expr
{
    public ListExpr(IEnumerable<Expr> items) { Items = new List<Expr>(items ?? Array.Empty<Expr>()); }
    public List<Expr> Items { get; }
    public override string ToString() => $"({string.Join(", ", Items)})";
}