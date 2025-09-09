using ToSic.Eav.DataSource;
using ToSic.Eav.WebApi.Sys.Admin.OData;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration? config)
    {
        if (config == null)
            return;

        var extraParams = config.Parse(ODataParams);
        var selectRaw = extraParams[EavWebApiConstants.ODataSelectParamName];
        var selectList = SystemQueryOptionsParser.ParseSelect(selectRaw);

        SelectFields = selectList.ToListOpt();
    }

    public ICollection<string> SelectFields { get; set; } = [];

    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [EavWebApiConstants.ODataSelectParamName] = $"[QueryString:{EavWebApiConstants.ODataSelectParamName}]",
            [EavWebApiConstants.ODataExpandParamName] = $"[QueryString:{EavWebApiConstants.ODataExpandParamName}]",
            [EavWebApiConstants.ODataFilterParamName] = $"[QueryString:{EavWebApiConstants.ODataFilterParamName}]",
            [EavWebApiConstants.ODataOrderByParamName] = $"[QueryString:{EavWebApiConstants.ODataOrderByParamName}]",
            [EavWebApiConstants.ODataTopParamName] = $"[QueryString:{EavWebApiConstants.ODataTopParamName}]",
            [EavWebApiConstants.ODataSkipParamName] = $"[QueryString:{EavWebApiConstants.ODataSkipParamName}]",
            [EavWebApiConstants.ODataCountParamName] = $"[QueryString:{EavWebApiConstants.ODataCountParamName}]"
        };
}