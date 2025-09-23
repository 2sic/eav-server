namespace ToSic.Sys.OData.Ast;

public sealed class CallExpr : Expr
{
    public string Name { get; }
    public IReadOnlyList<Expr> Arguments { get; }

    public CallExpr(string name, IReadOnlyList<Expr> args)
    {
        Name = name; Arguments = args;
    }

    public override string ToString() => $"{Name}({string.Join(", ", Arguments)})";
}