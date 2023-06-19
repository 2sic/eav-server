using System.Collections.Concurrent;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.CodeChanges
{
    public class CodeChangeStats
    {
        /// <summary>
        /// Empty for DI
        /// </summary>
        public CodeChangeStats() { }

        public void Register(LogStoreEntry use)
        {
            // We need the specs
            if (use?.Specs == null) return;
            if (!use.Specs.TryGetValue("AppId", out var appIdStr)) return;
            var appId = appIdStr.ConvertOrDefault<int>();
            if (appId == 0) return;
            ByAppCache[appId] = true;
        }

        public bool AppHasWarnings(int appId) => ByAppCache.ContainsKey(appId);

        private static IDictionary<int, bool> ByAppCache = new ConcurrentDictionary<int, bool>();
    }
}
