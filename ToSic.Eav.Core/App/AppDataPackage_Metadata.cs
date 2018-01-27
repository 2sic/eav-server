using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    /// <inheritdoc cref="IMetadataProvider" />
    /// <summary>
    /// Cache Object for a specific App
    /// </summary>
    public partial class AppDataPackage: IMetadataProvider
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
	    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
            => Metadata.GetMetadata(targetType, key, contentTypeName);


    }
}