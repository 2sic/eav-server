namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class ListExpr(IEnumerable<Expr> items) : Expr
{
    public List<Expr> Items { get; } = [..items ?? []];
    public override string ToString() => $"({string.Join(", ", Items)})";
}