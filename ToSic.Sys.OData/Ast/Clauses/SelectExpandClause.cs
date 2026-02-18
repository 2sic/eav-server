namespace ToSic.Sys.OData.Ast;

public sealed class SelectExpandClause
{
    public List<string> Select { get; init; } = [];
    public List<string> Expand { get; init; } = [];
}