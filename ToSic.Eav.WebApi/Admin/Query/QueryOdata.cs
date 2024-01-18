using ToSic.Eav.DataSource;

namespace ToSic.Eav.WebApi.Admin.Query;

internal class QueryOdata
{
    public const string FieldSelect = "$select";
    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [FieldSelect] = $"[QueryString:{FieldSelect}]"
        };
}

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration config)
    {
        if (config == null) return;
        var extraParams = config.Parse(QueryOdata.ODataParams);
        SelectFields = (extraParams[QueryOdata.FieldSelect]?.CsvToArrayWithoutEmpty() ?? []).ToList();

    }
    public List<string> SelectFields { get; set; }
}