using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSources.Caching
{
    public class CacheKey: ICacheKeyManager
    {
        public readonly IDataSource DataSource;
        public CacheKey(IDataSource dataSource) => DataSource = dataSource;


        /// <inheritdoc />
        public virtual string CachePartialKey
        {
            get
            {
                // Assemble the partial key
                // If this item has a guid that it's a configured part which always has this unique guid; then use that
                var key = DataSource.Name + ":" + (DataSource.Guid != Guid.Empty
                    ? DataSource.Guid.ToString()
                    : "NoGuid");

                // Important to check configuration first - to ensure all tokens are resolved to the resulting parameters
                DataSource.Configuration.Parse();

                // note: whenever a item has filter-parameters, these should be part of the key as well...

                return DataSource.CacheRelevantConfigurations
                    .Aggregate(key, (current, configName) => 
                        current + "&" + configName + "=" + DataSource.Configuration.Values[configName]);
            }
        }

        public virtual string CacheFullKey => _fullKey ?? (_fullKey = string.Join(">", SubKeys.Distinct()));


        /// <summary>
        /// make sure we don't re-create many keys, of if some streams have the same DataSource, only get the key once
        /// </summary>
        /// <returns></returns>
        private List<IDataSource> UniqueSources()
        {
            if (!(DataSource is IDataSourceTarget target)) return new List<IDataSource>();

            if (target.In == null || target.In.Count == 0)
                return new List<IDataSource>();

            return target.In
                .Select(pairs => pairs.Value?.Source)
                .Where(stream => stream != null)
                .Distinct()
                .ToList();
        }


        public string[] SubKeys
        {
            get
            {
                if (_dependentPartials != null) return _dependentPartials;

                var keys = UniqueSources()
                    .SelectMany(inStream => inStream.CacheKey.SubKeys)
                    .ToList();
                keys.Add(CachePartialKey);
                _dependentPartials = keys.ToArray();
                return _dependentPartials;
            }
        }

        private string[] _dependentPartials = null;

        private string _fullKey;
    }
}
