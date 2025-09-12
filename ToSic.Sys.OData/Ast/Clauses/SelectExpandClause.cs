namespace ToSic.Sys.OData.Ast;

public sealed class SelectExpandClause
{
    public List<string> Select { get; } = new List<string>();
    public List<string> Expand { get; } = new List<string>();
}