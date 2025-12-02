namespace ToSic.Sys.OData.Ast;

public sealed class CallExpr(string name, IReadOnlyList<Expr> args) : Expr
{
    public string Name { get; } = name;

    public IReadOnlyList<Expr> Arguments { get; } = args;

    public override string ToString() => $"{Name}({string.Join(", ", Arguments)})";
}