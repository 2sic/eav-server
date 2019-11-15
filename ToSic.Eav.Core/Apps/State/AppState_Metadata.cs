using System.Collections.Generic;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    /// <inheritdoc cref="IMetadataSource" />
    /// <summary>
    /// Cache Object for a specific App
    /// </summary>
    public partial class AppState: IMetadataSource
	{
        internal AppMetadataManager Metadata { get; set; }

        /// <summary>
        /// Get metadata-items of something
        /// </summary>
        /// <typeparam name="TMetadataKey">Type - guid, int or string</typeparam>
        /// <param name="targetType">target-type is a number from 1-4 which says if it's metadata of an entity, of an attribute, etc.</param>
        /// <param name="key">the (int/guid/string) key we're looking for</param>
        /// <param name="contentTypeName">an optional type name, if we only want the items of a specific type</param>
        /// <returns></returns>
	    public IEnumerable<IEntity> Get<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
            => Metadata.Get(targetType, key, contentTypeName);


    }
}