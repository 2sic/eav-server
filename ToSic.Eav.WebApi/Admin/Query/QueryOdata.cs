using ToSic.Eav.DataSource;
using ToSic.Sys.Utils;

namespace ToSic.Eav.WebApi.Admin.Query;

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration config)
    {
        if (config == null) return;
        var extraParams = config.Parse(ODataParams);
        SelectFields = (extraParams[EavWebApiConstants.ODataSelect]?.CsvToArrayWithoutEmpty() ?? []).ToList();

    }

    public List<string> SelectFields { get; set; }

    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [EavWebApiConstants.ODataSelect] = $"[QueryString:{EavWebApiConstants.ODataSelect}]"
        };
}