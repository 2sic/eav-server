using ToSic.Eav.DataSource.Internal.Query;

namespace ToSic.Eav.DataSource.Internal.Inspect;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class InspectQuery: ServiceBase
{
    /// <summary>
    /// DI Constructor
    /// </summary>
    public InspectQuery() : base("Qry.Info") { }
        
    public InspectQuery BuildQueryInfo(QueryDefinition queryDef, IDataSource queryResult)
    {
        QueryDefinition = queryDef;
        GetStreamInfosRecursive(queryResult);
        return this;
    }

    public QueryDefinition QueryDefinition;
    public List<InspectStream> Streams = [];
    public Dictionary<Guid, InspectDataSource> Sources = [];

    /// <summary>
    /// Provide an array of infos related to a stream and data source
    /// </summary>
    private void GetStreamInfosRecursive(IDataSource target) => Log.Do($"{target.Guid}[{target.In.Count}]", timer: true, action: l =>
    {
        foreach (var stream in target.In)
        {
            // First get all the streams (do this first so they stay together)
            try
            {
                var stmInfo = new InspectStream(stream.Value, target, stream.Key);
                if (Streams.Any(existing => existing.Equals(stmInfo)))
                    continue;
                Streams.Add(stmInfo);
            }
            catch
            {
                l.A("Error trying to build list of streams on DS");
            }

            // Try to add the target to Data-Source-Stats;
            try
            {
                var di = new InspectDataSource(target);
                if (!Sources.ContainsKey(di.Guid))
                    Sources.Add(di.Guid, di.WithQueryDef(QueryDefinition));
            }
            catch
            {
                l.A("Error adding target lists");
            }

            // Try to add the source to the data-source-stats
            try
            {
                var di = new InspectDataSource(stream.Value.Source);
                if (!Sources.ContainsKey(di.Guid))
                    Sources.Add(di.Guid, di.WithQueryDef(QueryDefinition));
            }
            catch
            {
                l.A("Error adding DataSourceInfo");
            }

            // Get Sub-Streams recursive
            try
            {
                GetStreamInfosRecursive(stream.Value.Source);
            }
            catch
            {
                l.A("Error in recursion");
            }
        }
    });
}