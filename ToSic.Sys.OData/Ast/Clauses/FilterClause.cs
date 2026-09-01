namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class FilterClause
{
    public Expr? Expression { get; init; }
}