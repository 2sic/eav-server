using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// For querying metadata from the data source.
    /// Mainly used in the Store, Cache-Systems and Apps.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IMetadataSource: 
        ICacheExpiring, 
        IAppIdentity    // this is used for creating additional metadata on this source
    {
        /// <summary>
        /// Get any metadata from the current data source (usually an app)
        /// </summary>
        /// <remarks>
        /// It does not return metadata describing the source (app), but metadata stored in this app, describing something else.
        /// </remarks>
        /// <typeparam name="TKey">Key-Type used - string, int or guid</typeparam>
        /// <param name="targetType">The type-id of the target we're accessing, like 10=cms-object, 4=entity</param>
        /// <param name="key">The key which is used for lookup</param>
        /// <param name="contentTypeName">Optional content-type name, to only retrieve metadata of that type</param>
        /// <returns>A list (may be empty) of metadata items found for this target</returns>
        IEnumerable<IEntity> GetMetadata<TKey>(int targetType, TKey key, string contentTypeName = null);

        /// <summary>
        /// Get any metadata from the current data source (usually an app)
        /// </summary>
        /// <remarks>
        /// It does not return metadata describing the source (app), but metadata stored in this app, describing something else.
        /// </remarks>
        /// <typeparam name="TKey">Key-Type used - string, int or guid</typeparam>
        /// <param name="targetType">The TargetTypes constant of the target we're accessing, like TargetTypes.Entity</param>
        /// <param name="key">The key which is used for lookup</param>
        /// <param name="contentTypeName">Optional content-type name, to only retrieve metadata of that type</param>
        /// <returns>A list (may be empty) of metadata items found for this target</returns>
        IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null);

    }
}
