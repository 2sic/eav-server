namespace ToSic.Eav.WebApi.Sys.Admin.Query;

/// <summary>
/// Optional additional parameters in a query post body
/// </summary>
public class QueryParametersDtoFromClient
{
    /// <summary>
    /// Optional parameters to specify certain guids to filter the query by.
    /// </summary>
    public string[]? Guids { get; init; }
}