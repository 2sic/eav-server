namespace ToSic.Sys.OData.Ast;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class IdentifierExpr(string name) : Expr
{
    public string Name { get; } = name;
    public override string ToString() => Name;
}