﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
    public class GlobalQueries
    {
        private readonly Lazy<IRuntime> _runtimeLazy;
        public const string GlobalEavQueryPrefix = "Eav.Queries.Global.";
        public const string GlobalQueryPrefix = "Global.";

        #region Constructor / DI

        public GlobalQueries(Lazy<IRuntime> runtimeLazy)
        {
            _runtimeLazy = runtimeLazy;
        }

        #endregion

        /// <summary>
        /// List of global queries, caches after first scan
        /// </summary>
        /// <returns></returns>
        public List<IEntity> AllQueries()
        {
            if (_globalQueriesCache != null) return _globalQueriesCache;

            // add runtime stuff
            var runtimeType = QueriesInRuntime().ToList();

            // make sure it's case insensitive...
            _globalQueriesCache = runtimeType;
            return _globalQueriesCache;
        }
        private static List<IEntity> _globalQueriesCache;


        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEntity> QueriesInRuntime()
        {
            if (_runtimeCache != null) return _runtimeCache;

            var runtime = _runtimeLazy.Value.Init(null);
            _runtimeCache = runtime?.LoadGlobalItems("query") ?? new List<IEntity>();
            return _runtimeCache;
        }

        private static IEnumerable<IEntity> _runtimeCache;


        public IEntity FindQuery(string name)
        {
            // Note: must use GetBestValue("Name") as GetTitle() will fail, since the type is often not known
            var types = _globalQueriesCache ?? AllQueries();
            return types.FirstOrDefault(e => e.Value<string>("Name") == name);
        }
    }
}
