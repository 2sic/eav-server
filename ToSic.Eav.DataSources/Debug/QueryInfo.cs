using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSources.Debug
{
    public class QueryInfo: ServiceBase
    {
        /// <summary>
        /// DI Constructor
        /// </summary>
        public QueryInfo() : base("Qry.Info") { }
        
        public QueryInfo BuildQueryInfo(QueryDefinition queryDef, IDataSource queryResult)
        {
            QueryDefinition = queryDef;
            GetStreamInfosRecursive(queryResult as IDataTarget);
            return this;
        }

        public QueryDefinition QueryDefinition;
        public List<StreamInfo> Streams = new List<StreamInfo>();
        public Dictionary<Guid, DataSourceInfo> Sources = new Dictionary<Guid, DataSourceInfo>();

        /// <summary>
        /// Provide an array of infos related to a stream and data source
        /// </summary>
        private void GetStreamInfosRecursive(IDataTarget target) => Log.Do($"{target.Guid}[{target.In.Count}]", timer: true, action: l =>
        {
            foreach (var stream in target.In)
            {
                // First get all the streams (do this first so they stay together)
                try
                {
                    var stmInfo = new StreamInfo(stream.Value, target, stream.Key);
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
                    var di = new DataSourceInfo(target as IDataSource);
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
                    var di = new DataSourceInfo(stream.Value.Source);
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
                    GetStreamInfosRecursive(stream.Value.Source as IDataTarget);
                }
                catch
                {
                    l.A("Error in recursion");
                }
            }
        });
    }
}
