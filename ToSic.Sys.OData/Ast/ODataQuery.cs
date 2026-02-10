namespace ToSic.Sys.OData.Ast;

// Root container for parsed system query options
public sealed class ODataQuery
{
    public FilterClause? Filter { get; init; }
    public OrderByClause? OrderBy { get; init; }
    public SelectExpandClause? SelectExpand { get; init; }
    public SearchClause? Search { get; init; }
    public ComputeClause? Compute { get; init; }
    public long? Top { get; init; }
    public long? Skip { get; init; }
    public long? Index { get; init; }
    public bool? Count { get; init; }
    public string? SkipToken { get; init; }
    public string? DeltaToken { get; init; }
}