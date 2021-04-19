using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataSources.Debug
{
    public class DataSourceInfo
    {
        public Guid Guid;
        public string Type;
        public IDictionary<string, string> Configuration;
        public bool Error = false;

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
    }
}
