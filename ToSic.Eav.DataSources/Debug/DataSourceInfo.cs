using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources.Debug
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
        public IDictionary<string, string> Configuration { get; }

        /// <summary>
        /// Error state
        /// </summary>
        public bool Error { get; set; }

        public Dictionary<string, object> Definition;

        public Connections Connections { get; set; }

        public DataSourceInfo(IDataSource ds)
        {
            try
            {
                Guid = ds.Guid;
                Type = ds.GetType().Name;
                Connections = (ds as DataSourceBase)?.Connections;
                Configuration = ds.Configuration.Values;
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
}
