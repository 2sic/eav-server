using ToSic.Eav.DataSource;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration config)
    {
        if (config == null) return;
        var extraParams = config.Parse(ODataParams);
        SelectFields = (extraParams[EavWebApiConstants.ODataSelectParamName]?.CsvToArrayWithoutEmpty() ?? []).ToList();

    }

    public List<string> SelectFields { get; set; }

    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [EavWebApiConstants.ODataSelectParamName] = $"[QueryString:{EavWebApiConstants.ODataSelectParamName}]"
        };
}