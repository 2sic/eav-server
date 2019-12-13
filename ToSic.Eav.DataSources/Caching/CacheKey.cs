using System;
using System.Linq;
using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSources.Caching
{
    public class CacheKey: ICacheKey
    {
        public readonly DataSourceBase DataSource;
        public CacheKey(DataSourceBase dataSource)
        {
            DataSource = dataSource;
        }


        /// <inheritdoc />
        public string CachePartialKey
        {
            get
            {
                // Assemble the partial key
                // If this item has a guid thet it's a configured part which always has this unique guid; then use that
                var key = DataSource.Guid != Guid.Empty
                    ? DataSource.Name + DataSource.Guid
                    : DataSource.Name + "-NoGuid";

                // Important to check configuration first - to ensure all tokens are resolved to the resulting parameters
                DataSource.ConfigurationParse();

                // note: whenever a item has filter-parameters, these should be part of the key as well...

                return DataSource.CacheRelevantConfigurations
                    .Aggregate(key, (current, configName) => 
                        current + "&" + configName + "=" + DataSource.Configuration.Values[configName]);
            }
        }

        /// <inheritdoc />
        public string CacheFullKey
        {
            get
            {
                var fullKey = "";

                // If there is an upstream, use that as the leading part of the id
                if (DataSource.In.ContainsKey(Constants.DefaultStreamName) && DataSource.In[Constants.DefaultStreamName] != null)
                    fullKey += DataSource.In[Constants.DefaultStreamName].Source.CacheFullKey + ">";

                // add current key
                fullKey += CachePartialKey;
                return fullKey;
            }
        }
    }
}
