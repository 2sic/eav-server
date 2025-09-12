namespace ToSic.Sys.OData.Ast;

public sealed class SearchClause
{
    public Expr Expression { get; set; } // using the same Expr for AND/OR/TERM/PHRASE
}