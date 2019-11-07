using System;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp
{
	/// <summary>
	/// LookUp things from metadata. This uses EAV Metadata system and will look up Metadata for something.
	/// As of now it's hardwired to look up Metadata of Entities
	/// </summary>
	[PublicApi]
	public class LookUpInMetadata : LookUpInEntity
	{
	    private readonly IMetadataProvider _metaDataSource;
		private readonly Guid _objectToProvideSettingsTo;
		private bool _entityLoaded;
	    private readonly IEntity _parent;


	    /// <summary>
		/// Constructs the object with pre-filled parameters. It won't access the entity yet, because 
		/// it's possible that the data-source wouldn't be ready yet. The access to the entity will 
		/// only occur if it's really needed. 
		/// </summary>
		/// <param name="name">Name of the PropertyAccess, e.g. PipelineSettings</param>
		/// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
		/// <param name="metaDataSource">DataSource that provides MetaData</param>
		public LookUpInMetadata(string name, Guid objectId, IMetadataProvider metaDataSource)
		{
			Name = name;
			_objectToProvideSettingsTo = objectId;
			_metaDataSource = metaDataSource;
		}

        /// <summary>
        /// Alternate constructor where the entity with attached metadata is already known.
        /// The attached metadata will be used as source for the look-up
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="entityWithMetadata">Entity whose metadata we'll use</param>
		public LookUpInMetadata(string name, IEntity entityWithMetadata)
		{
			Name = name;
		    _parent = entityWithMetadata;
		}

        /// <summary>
        /// For late-loading the entity. Will be called automatically by the Get if not loaded yet. 
        /// </summary>
        [PrivateApi]
		protected void LoadEntity()
        {
            var md = _parent?.Metadata ??
                     _metaDataSource.GetMetadata(Constants.MetadataForEntity, _objectToProvideSettingsTo);

            // make sure we get the settings, but not the pipeline-parts, which may also be assigned
			Entity = md.FirstOrDefault(e => e.Type.StaticName != Constants.QueryPartTypeName);
			_entityLoaded = true;
		}

        /// <inheritdoc/>
        public override string Get(string key, string format, ref bool notFound)
        {
            if (!_entityLoaded)
                LoadEntity();

            return base.Get(key, format, ref notFound);
        }
	}
}