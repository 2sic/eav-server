using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Debug
{
    public class PipelineInfo
    {
        public List<StreamInfo> Streams = new List<StreamInfo>();
        public Dictionary<Guid, DataSourceInfo> Sources = new Dictionary<Guid, DataSourceInfo>();

        public PipelineInfo(IDataSource outStream)
        {
            GetStreamInfosRecursive(outStream as IDataTarget, ref Streams, ref Sources);
        }

        /// <summary>
        /// Provide an array of infos related to a stream and data source
        /// </summary>
        public static void GetStreamInfosRecursive(IDataTarget source, ref List<StreamInfo> statsList, ref Dictionary<Guid, DataSourceInfo> dsStats)
        {
            // ReSharper disable EmptyGeneralCatchClause
            foreach (var strm in source.In)
            {
                // First get all the stats (do this first so they stay together)
                //var stmInfo = new StreamInfo();
                try
                {
                    var stmInfo = new StreamInfo(strm.Value, source, strm.Key);
                    statsList.Add(stmInfo);

                }
                catch { }

                // Try to add the target to Data-Source-Stats
                try
                {
                    var di = new DataSourceInfo(source as IDataSource);
                    if (!dsStats.ContainsKey(di.Guid))
                        dsStats.Add(di.Guid, di);
                }
                catch { }

                // Try to add the source to the data-source-stats
                try
                {
                    var di = new DataSourceInfo(strm.Value.Source);
                    if (!dsStats.ContainsKey(di.Guid))
                        dsStats.Add(di.Guid, di);
                }
                catch { }

                // then go to the substreams, try those
                try
                {
                    GetStreamInfosRecursive(strm.Value.Source as IDataTarget, ref statsList, ref dsStats);
                }
                catch { }
            }
            // ReSharper restore EmptyGeneralCatchClause

        }
    }
}
