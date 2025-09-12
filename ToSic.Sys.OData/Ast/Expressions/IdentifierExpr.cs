namespace ToSic.Sys.OData.Ast;

public sealed class IdentifierExpr : Expr
{
    public IdentifierExpr(string name) { Name = name; }
    public string Name { get; }
    public override string ToString() => Name;
}