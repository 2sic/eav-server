using System.Diagnostics;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSource.Query.Sys.Inspect;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Sys.OData;

namespace ToSic.Eav.DataSources.Query.Dev.Sys;

/// <summary>
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryDevSvc(QueryFactory queryFactory, QueryDefinitionService queryDefSvc, IConvertToEavLight entToJson, QueryInspectionService queryInfoLazy)
    : ServiceBase("Qry.Dev", connect: [queryFactory, queryDefSvc, entToJson, queryInfoLazy])
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="id"></param>
    /// <param name="lookUps"></param>
    /// <param name="top"></param>
    /// <param name="partLookup">This retrieves which part of the query should be provided - for scenarios where only a single stream in the query is analyzed</param>
    /// <returns></returns>
    public QueryRunDto RunDev(int appId, int id, ILookUpEngine lookUps, int top, Func<QueryFactoryResult, IDataSource> partLookup)
    {
        var l = Log.Fn<QueryRunDto>($"a#{appId}, {nameof(id)}:{id}, top: {top}");

        // Get the query, run it and track how much time this took
        var qDef = queryDefSvc.GetDefinition(appId, id);
        var builtQuery = queryFactory.CreateWithTestParams(qDef, lookUps: lookUps);
        var outSource = builtQuery.Main;

        // New v17 experimental with special fields
        var systemQueryOptions = QueryODataParams.Create(outSource.Configuration.Parse);

        var timer = new Stopwatch();
        timer.Start();
        var results = Log.Quick(message: "Serialize", timer: true, func: () =>
        {
            entToJson.MaxItems = top;
            entToJson.AddSelectFields(systemQueryOptions.Select.ToListOpt());

            // Use passed in function to select the part to serialize
            var part = partLookup(builtQuery);
            var converted = entToJson.Convert(part);
            return converted;
        });
        timer.Stop();

        // Now get some more debug info
        var debugInfo = queryInfoLazy.BuildQueryInfo(qDef, outSource);

        // ...and return the results
        var result = new QueryRunDto
        {
            Query = results,
            Streams = debugInfo.Streams
                .Select(si =>
                {
                    if (si.ErrorData is IEntity errorEntity)
                        si.ErrorData = entToJson.Convert(errorEntity);
                    return si;
                })
                .ToListOpt(),
            Sources = debugInfo.Sources,
            QueryTimer = new()
            {
                Milliseconds = timer.ElapsedMilliseconds,
                Ticks = timer.ElapsedTicks
            }
        };
        return l.ReturnAsOk(result);
    }

}
