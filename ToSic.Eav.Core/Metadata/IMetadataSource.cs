using System.Collections.Generic;
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
    public interface IMetadataSource: ICacheExpiring
    {
        // FYI: disabled 2021-11-19, was deprecated since v11.11 #cleanup EOY 2021
        //[Obsolete("Deprecated in 11.11. Use GetMetadata instead. just keeping this till 2sxc 12 for compatibility")]
        //IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null);

        /// <summary>
        /// Get any metadata from the current data source (usually an app)
        /// </summary>
        /// <remarks>
        /// It does not return metadata describing the source (app), but metadata stored in this app, describing something else.
        /// </remarks>
        /// <typeparam name="T">Key-Type used - string, int or guid</typeparam>
        /// <param name="targetType">The type-id of the target we're accessing, like 10=cms-object, 4=entity</param>
        /// <param name="key">The key which is used for lookup</param>
        /// <param name="contentTypeName">Optional content-type name, to only retrieve metadata of that type</param>
        /// <returns>A list (may be empty) of metadata items found for this target</returns>
        IEnumerable<IEntity> GetMetadata<T>(int targetType, T key, string contentTypeName = null);

    }
}
