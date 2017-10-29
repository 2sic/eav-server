using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.App
{
	/// <inheritdoc />
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
    

        public IEnumerable<IEntity> GetMetadata(int targetType, int key, string contentTypeName = null) => Lookup(MetadataForNumber, targetType, Convert.ToInt32(key), contentTypeName);

	    public IEnumerable<IEntity> GetMetadata(int targetType, string key, string contentTypeName = null) => Lookup(MetadataForString, targetType, key, contentTypeName);

	    public IEnumerable<IEntity> GetMetadata(int targetType, Guid key, string contentTypeName = null) => Lookup(MetadataForGuid, targetType, key, contentTypeName);

	    private IEnumerable<IEntity> Lookup<T>(IDictionary<int, Dictionary<T, IEnumerable<IEntity>>> list, int targetType, T key, string contentTypeName = null)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            if (list.TryGetValue(targetType, out Dictionary<T, IEnumerable<IEntity>> keyGuidDictionary))
                if (keyGuidDictionary.TryGetValue(key, out IEnumerable<IEntity> entities))
                    return entities.Where(e => contentTypeName == null || e.Type.StaticName == contentTypeName);
            return new List<IEntity>();
        }
        #endregion



	}
}