using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// For querying metadata from the data source.
    /// Mainly used in the Store, Cache-Systems and Apps.
    /// </summary>
    [PublicApi]
    public interface IMetadataProvider: ICacheExpiring
    {
        /// <summary>
        /// Get any metadata of the current data source (usually an app)
        /// </summary>
        /// <typeparam name="T">Key-Type used - string, int or guid</typeparam>
        /// <param name="targetType">The type-id of the target we're accessing, like 10=cms-object, 4=entity</param>
        /// <param name="key">The key which is used for lookup</param>
        /// <param name="contentTypeName">Optional content-type name, to only retrieve metadata of that type</param>
        /// <returns>A list (may be empty) of metadata items found for this target</returns>
        IEnumerable<IEntity> GetMetadata<T>(int targetType, T key, string contentTypeName = null);

    }
}
