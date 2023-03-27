using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSource.Debug
{
    public class DataSourceInfo
    {
        /// <summary>
        /// DS Guid for identification
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Internal type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The resolved values used from the settings/config of the data source
        /// </summary>
        public IReadOnlyDictionary<string, string> Configuration { get; }

        /// <summary>
        /// Error state
        /// </summary>
        public bool Error { get; set; }

        public Dictionary<string, object> Definition;

        public List<OutDto> Out;

        public DataSourceConnections Connections { get; set; }

        public DataSourceInfo(IDataSource ds)
        {
            try
            {
                Guid = ds.Guid;
                Type = ds.GetType().Name;
                Connections = (ds as DataSources.DataSource)?.Connections;
                Configuration = ds.Configuration.Values;

                try
                {
                    Out = ds.Out.Select(o => new OutDto { Name = o.Key, Scope = o.Value.Scope })
                        .ToList();
                }
                catch { /* ignore */ }
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        public DataSourceInfo WithQueryDef(QueryDefinition queryDefinition)
        {
            // find this item in the query def
            var partDef = queryDefinition.Parts.FirstOrDefault(p => p.Guid == Guid);
            if(partDef is null) return this;

            Definition = partDef.AsDictionary();
            return this;
        }
    }

    public class OutDto
    {
        public string Name { get; set; }
        public string Scope { get; set; }
    }
}
