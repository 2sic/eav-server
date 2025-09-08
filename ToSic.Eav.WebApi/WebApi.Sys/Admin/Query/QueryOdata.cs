using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Text.RegularExpressions;
using ToSic.Eav.DataSource;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

internal class QueryODataParams
{
    public QueryODataParams(IDataSourceConfiguration? config)
    {
        if (config == null)
            return;
        var extraParams = config.Parse(ODataParams);

        // TODO: @stv ODataParse from uri

        //// POC for OData parsing
        //var uri = ODataParse(extraParams[EavWebApiConstants.ODataSelectParamName]);

        //// --- What you get back ---
        //var path = uri.Path; // EntitySet: BlogPost
        //var select = GetSelectedProperties(uri.SelectAndExpand); // ["Title","Content"]
        //var filter = uri.Filter;   // null for this sample
        //var order = uri.OrderBy;  // null for this sample
        //var top = uri.Top;      // null …
        //var skip = uri.Skip;     // null …

        //// Custom (non-OData) options like PageId/ModuleId are preserved here:
        //foreach (var opt in uri.CustomQueryOptions)
        //{
        //    // opt.Name == "PageId"/"ModuleId", opt.Value == "3065"/"10401"
        //}


        SelectFields = (extraParams[EavWebApiConstants.ODataSelectParamName]?.CsvToArrayWithoutEmpty() ?? [])
            .ToListOpt();

    }

    public ICollection<string> SelectFields { get; set; } = [];

    public static Dictionary<string, string> ODataParams =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            [EavWebApiConstants.ODataSelectParamName] = $"[QueryString:{EavWebApiConstants.ODataSelectParamName}]"
        };
}