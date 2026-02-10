namespace ToSic.Sys.OData.Ast;

// Root container for parsed system query options
public sealed class ODataQuery
{
    public FilterClause? Filter { get; set; }
    public OrderByClause? OrderBy { get; set; }
    public SelectExpandClause? SelectExpand { get; set; }
    public SearchClause? Search { get; set; }
    public ComputeClause? Compute { get; set; }
    public long? Top { get; set; }
    public long? Skip { get; set; }
    public long? Index { get; set; }
    public bool? Count { get; set; }
    public string? SkipToken { get; set; }
    public string? DeltaToken { get; set; }
}