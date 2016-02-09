using System;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Get Values from Assigned Entities
	/// </summary>
	public class AssignedEntityValueProvider : EntityValueProvider // IValueProvider
	{
		private readonly string _name;
		private readonly IMetaDataSource _metaDataSource;
		private readonly Guid _objectToProvideSettingsTo;
		private IEntity _assignedEntity;
		private bool _entityLoaded;

		public string Name { get { return _name; } }

		/// <summary>
		/// Constructs the object with prefilled parameters. It won't access the entity yet, because 
		/// it's possible that the data-source wouldn't be ready yet. The access to the entity will 
		/// only occur if it's really needed. 
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. pipelinesettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
		public AssignedEntityValueProvider(string name, Guid objectId, IMetaDataSource metaDataSource)
		{
			_name = name;
			_objectToProvideSettingsTo = objectId;
			_metaDataSource = metaDataSource;
		}

        /// <summary>
        /// For late-loading the entity. Will be called automatically by the Get if not loaded yet. 
        /// </summary>
		protected void LoadEntity()
		{
			var assignedEntities = _metaDataSource.GetAssignedEntities(Constants.AssignmentObjectTypeEntity, _objectToProvideSettingsTo);
			_entity = assignedEntities.FirstOrDefault(e => e.Type.StaticName != Constants.DataPipelinePartStaticName);
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
            //try
            //{
            //    return _assignedEntity[property][0].ToString();
            //}
            //catch
            //{
            //    propertyNotFound = true;
            //    return string.Empty;
            //}
        }
	}
}