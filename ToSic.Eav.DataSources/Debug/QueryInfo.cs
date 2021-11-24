using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources.Debug
{
    public class QueryInfo: HasLog
    {
        /// <summary>
        /// DI Constructor
        /// </summary>
        public QueryInfo() : base("Qry.Info") { }
        
        public QueryInfo BuildQueryInfo(QueryDefinition queryDef, IDataSource queryResult, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            QueryDefinition = queryDef;
            GetStreamInfosRecursive(queryResult as IDataTarget, ref Streams, ref Sources);
            return this;
        }

        public QueryDefinition QueryDefinition;
        public List<StreamInfo> Streams = new List<StreamInfo>();
        public Dictionary<Guid, DataSourceInfo> Sources = new Dictionary<Guid, DataSourceInfo>();

        /// <summary>
        /// Provide an array of infos related to a stream and data source
        /// </summary>
        private void GetStreamInfosRecursive(IDataTarget target, ref List<StreamInfo> streams, ref Dictionary<Guid, DataSourceInfo> sources)
        {
            var wrapLog = Log.Call($"{target.Guid}[{target.In.Count}]", useTimer: true);

            foreach (var stream in target.In)
            {
                // First get all the streams (do this first so they stay together)
                try
                {
                    var stmInfo = new StreamInfo(stream.Value, target, stream.Key);
                    if (streams.Any(existing => existing.Equals(stmInfo)))
                        continue;
                    streams.Add(stmInfo);
                }
                catch
                {
                    Log.Add("Error trying to build list of streams on DS");
                }

                // Try to add the target to Data-Source-Stats;
                try
                {
                    var di = new DataSourceInfo(target as IDataSource);
                    if (!sources.ContainsKey(di.Guid))
                        sources.Add(di.Guid, di.WithQueryDef(QueryDefinition));
                }
                catch
                {
                    Log.Add("Error adding target lists");
                }

                // Try to add the source to the data-source-stats
                try
                {
                    var di = new DataSourceInfo(stream.Value.Source);
                    if (!sources.ContainsKey(di.Guid))
                        sources.Add(di.Guid, di.WithQueryDef(QueryDefinition));
                }
                catch
                {
                    Log.Add("Error adding DataSourceInfo");
                }

                // Get Sub-Streams recursive
                try
                {
                    GetStreamInfosRecursive(stream.Value.Source as IDataTarget, ref streams, ref sources);
                }
                catch
                {
                    Log.Add("Error in recursion");
                }
            }

            wrapLog("ok");
        }
    }
}
