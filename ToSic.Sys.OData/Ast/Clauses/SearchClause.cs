namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class SearchClause
{
    public Expr? Expression { get; init; } // using the same Expr for AND/OR/TERM/PHRASE
}