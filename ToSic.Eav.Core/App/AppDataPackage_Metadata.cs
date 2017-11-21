using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
    /// <inheritdoc cref="IMetadataProvider" />
    /// <summary>
    /// Cache Object for a specific App
    /// </summary>
    public partial class AppDataPackage: IMetadataProvider
	{

        #region Metadata Properties
        /// <summary>
        /// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyGuid
        /// </summary>
        private IDictionary<int, Dictionary<Guid, IEnumerable<IEntity>>> MetadataForGuid { get; }

		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyNumber
		/// </summary>
		private IDictionary<int, Dictionary<int, IEnumerable<IEntity>>> MetadataForNumber { get; }

		/// <summary>
		/// Gets a Dictionary of AssignmentObjectTypes and assigned Entities having a KeyString
		/// </summary>
		private IDictionary<int, Dictionary<string, IEnumerable<IEntity>>> MetadataForString { get; }
    
        /// <summary>
        /// Get metadata-items of something
        /// </summary>
        /// <typeparam name="TMetadataKey">Type - guid, int or string</typeparam>
        /// <param name="targetType">target-type is a number from 1-4 which says if it's metadata of an entity, of an attribute, etc.</param>
        /// <param name="key">the (int/guid/string) key we're looking for</param>
        /// <param name="contentTypeName">an optional type name, if we only want the items of a specific type</param>
        /// <returns></returns>
	    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null)
	    {
	        if (typeof(TMetadataKey) == typeof(Guid))
	            return Lookup(MetadataForGuid, targetType, key as Guid? ?? Guid.Empty, contentTypeName);

	        switch (Type.GetTypeCode(typeof(TMetadataKey)))
	        {
	            case TypeCode.Int32:
	                return Lookup(MetadataForNumber, targetType, key as int? ?? 0, contentTypeName);
	            case TypeCode.String:
	                return Lookup(MetadataForString, targetType, key as string, contentTypeName);
	            default:
	                return Lookup(MetadataForString, targetType, key.ToString(), contentTypeName);
            }
	    }

	    private static IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, IEnumerable<IEntity>>> list, int targetType, T key, string contentTypeName)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            if (list.TryGetValue(targetType, out Dictionary<T, IEnumerable<IEntity>> keyDict))
                if (keyDict.TryGetValue(key, out IEnumerable<IEntity> entities))
                    return contentTypeName == null
                        ? entities
                        : entities.Where(e => e.Type.StaticName == contentTypeName);
            return new List<IEntity>();
        }
        #endregion



	}
}