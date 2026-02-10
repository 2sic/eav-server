namespace ToSic.Sys.OData.Ast;

public sealed class SelectExpandClause
{
    public List<string> Select { get; } = [];
    public List<string> Expand { get; } = [];
}