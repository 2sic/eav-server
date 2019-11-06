using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.ValueProviders
{
	/// <inheritdoc />
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class AssignedEntityValueProvider : EntityValueProvider
	{
	    private readonly IMetadataProvider _metaDataSource = null;
		private readonly Guid _objectToProvideSettingsTo;
		private bool _entityLoaded;
	    private IEntity _parent = null;

		//public new string Name { get; }



	    /// <summary>
		/// Constructs the object with prefilled parameters. It won't access the entity yet, because 
		/// it's possible that the data-source wouldn't be ready yet. The access to the entity will 
		/// only occur if it's really needed. 
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
		public AssignedEntityValueProvider(string name, Guid objectId, IMetadataProvider metaDataSource)
		{
			Name = name;
			_objectToProvideSettingsTo = objectId;
			_metaDataSource = metaDataSource;
		}
		public AssignedEntityValueProvider(string name, IEntity entityWithMetadata)
		{
			Name = name;
		    _parent = entityWithMetadata;
		}
        /// <summary>
        /// For late-loading the entity. Will be called automatically by the Get if not loaded yet. 
        /// </summary>
		protected void LoadEntity()
        {
            var md = _parent?.Metadata ??
                     _metaDataSource.GetMetadata(Constants.MetadataForEntity, _objectToProvideSettingsTo);

            // make sure we get the settings, but not the pipeline-parts, which may also be assigned
			Entity = md.FirstOrDefault(e => e.Type.StaticName != Constants.QueryPartTypeName);
			_entityLoaded = true;
		}

        /// <summary>
        /// Get Property of AssignedEntity
        /// </summary>
        /// <param name="property">Name of the Property</param>
        /// <param name="format">Format String</param>
        /// <param name="propertyNotFound">referenced Bool to set if Property was not found on AssignedEntity</param>
        public override string Get(string property, string format, ref bool propertyNotFound)
        {
            if (!_entityLoaded)
                LoadEntity();

            return base.Get(property, format, ref propertyNotFound);
        }
	}
}