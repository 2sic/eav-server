using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// A template object to build a global catalog of features or something.
    /// Goal is that it should be implemented by a specific class, 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GlobalCatalogBase<T>: HasLog, ILogShouldNeverConnect where T : IHasIdentityNameId
    {
        protected GlobalCatalogBase(History history, string logName, CodeRef code)
            : base(logName, code: code, initialMessage: $"Catalog Created for {typeof(T).Name}")
        {
            history.Add(LogNames.LogHistoryGlobalAndStartUp, Log);
        }

        /// <summary>
        /// The dictionary containing all the items.
        /// Case insensitive.
        /// </summary>
        public IReadOnlyDictionary<string, T> Dictionary { get; private set; }

        public IReadOnlyCollection<T> List { get; private set; }

        public T TryGet(string name) => Dictionary.TryGetValue(name, out var value) ? value : default;

        /// <summary>
        /// Add things to the registry
        /// </summary>
        /// <param name="items"></param>
        public void Register(params T[] items)
        {
            var wrapLog = Log.Fn($"Will add {items.Length} items");
            // add all features if it doesn't yet exist, otherwise update
            foreach (var f in items)
                if (f != null)
                {
                    Log.A($"Adding {f.NameId}");
                    _master.AddOrUpdate(f.NameId, f, (key, existing) => f);
                }

            // Reset the read-only dictionary
            Dictionary = new ReadOnlyDictionary<string, T>(_master);
            List = new ReadOnlyCollection<T>(_master.Values.ToList());
            wrapLog.Done($"now contains {List.Count} items");
        }

        private readonly ConcurrentDictionary<string, T> _master = new ConcurrentDictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
    }
}
