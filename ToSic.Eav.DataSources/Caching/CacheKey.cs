using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Caching;

namespace ToSic.Eav.DataSources.Caching
{
    public class CacheKey: ICacheKeyManager
    {
        public readonly DataSourceBase DataSource;
        public CacheKey(DataSourceBase dataSource) => DataSource = dataSource;


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

        //public string CacheFullKeyOldV1JustKeepInCaseOfProblems
        //{
        //    get
        //    {
        //        if (_cacheFullKey != null) return _cacheFullKey;

        //        var fullKey = "";

        //        // If there is an upstream, use that as the leading part of the id
        //        if (DataSource.In.ContainsKey(Constants.DefaultStreamName) &&
        //            DataSource.In[Constants.DefaultStreamName] != null)
        //            fullKey += DataSource.In[Constants.DefaultStreamName].Source.CacheFullKey + ">";

        //        // add current key
        //        fullKey += CachePartialKey;
        //        return _cacheFullKey = fullKey;
        //    }
        //}
        //private string _cacheFullKey;

        //public string[] DependentFullKeys
        //{
        //    get
        //    {
        //        if (_dependentFulls != null) return _dependentFulls;
        //        _dependentFulls = UniqueSources().Select(inStream => inStream.CacheFullKey).ToArray();
        //        return _dependentFulls;
        //    }
        //}
        //private string[] _dependentFulls = null;

        /// <summary>
        /// make sure we don't re-create many keys, of if some streams have the same DataSource, only get the key once
        /// </summary>
        /// <returns></returns>
        private List<IDataSource> UniqueSources()
        {
            if (DataSource.In == null || DataSource.In.Count == 0)
                return new List<IDataSource>();

            return DataSource.In
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

        //public string FullKey => _fullKey ?? (_fullKey = string.Join(">", SubKeys.Distinct()));

        private string _fullKey;
    }
}
