using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.LookUp.Sys.Engines;

namespace ToSic.Eav.DataSources.Query.Dev.Sys;

/// <summary>
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryDevDebugSvc(QueryDevSvc queryDev, Generator<PassThrough> passThrough)
    : ServiceBase("Qry.Dev", connect: [queryDev, passThrough])
{

    /// <summary>
    /// Query the Result of a Pipeline using Test-Parameters
    /// </summary>
    public QueryRunDto DebugStream(int appId, int id, int top, ILookUpEngine lookUps, string from, string streamName)
    {
        return queryDev.RunDev(appId, id, lookUps, top, GetSubStream);

        IDataSource GetSubStream(QueryFactoryResult builtQuery)
        {
            // Find the DataSource
            if (!builtQuery.DataSources.TryGetValue(from, out var source))
                throw new($"Can't find source with name '{from}'");

            if (!source.Out.TryGetValue(streamName, out var resultStream))
                throw new($"Can't find stream '{streamName}' on source '{from}'");

            // Repackage as DataSource since that's expected / needed
            var passThroughDs = passThrough.New();
            passThroughDs.Attach(streamName, resultStream);

            return passThroughDs;
        }
    }
}
