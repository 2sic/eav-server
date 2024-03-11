using ToSic.Eav.DataSource;

namespace ToSic.Eav.WebApi.Admin.Query;

internal class QueryOdata
{
    
    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [EavWebApiConstants.ODataSelect] = $"[QueryString:{EavWebApiConstants.ODataSelect}]"
        };
}

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration config)
    {
        if (config == null) return;
        var extraParams = config.Parse(QueryOdata.ODataParams);
        SelectFields = (extraParams[EavWebApiConstants.ODataSelect]?.CsvToArrayWithoutEmpty() ?? []).ToList();

    }
    public List<string> SelectFields { get; set; }
}