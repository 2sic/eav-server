using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// A template object to build a global catalog of features or something.
    /// Goal is that it should be implemented by a specific class, 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GlobalCatalogBase<T>
    {
        /// <summary>
        /// The dictionary containing all the items.
        /// Case insensitive.
        /// </summary>
        public IReadOnlyDictionary<string, T> Dictionary { get; private set; }

        /// <summary>
        /// Add things to the registry
        /// </summary>
        /// <param name="features"></param>
        public void Register(params T[] features)
        {
            // add all features if it doesn't yet exist, otherwise update
            foreach (var f in features)
                if (f != null)
                    _master.AddOrUpdate(GetKey(f), f, (key, existing) => f);

            // Reset the read-only dictionary
            Dictionary = new ReadOnlyDictionary<string, T>(_master);
        }

        private readonly ConcurrentDictionary<string, T> _master = new ConcurrentDictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        protected abstract string GetKey(T item);
    }
}
