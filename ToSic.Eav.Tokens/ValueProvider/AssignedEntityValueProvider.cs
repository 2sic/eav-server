using System;
using System.Linq;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ValueProvider
{
	/// <inheritdoc />
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class AssignedEntityValueProvider : EntityValueProvider
	{
	    private readonly IMetadataProvider _metaDataSource;
		private readonly Guid _objectToProvideSettingsTo;
		private bool _entityLoaded;

		public new string Name { get; }

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

        /// <summary>
        /// For late-loading the entity. Will be called automatically by the Get if not loaded yet. 
        /// </summary>
		protected void LoadEntity()
		{
			var assignedEntities = _metaDataSource.GetMetadata(Constants.MetadataForEntity, _objectToProvideSettingsTo);
			Entity = assignedEntities.FirstOrDefault(e => e.Type.StaticName != Constants.DataPipelinePartStaticName);
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