using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Experimental
    /// </summary>
    [PrivateApi]
    public class EntityWithSerialization: EntityDecorator, IEntitySerialization
    {
        public EntityWithSerialization(IEntity baseEntity) : base(baseEntity)
        {
        }

        public bool? SerializeId { get; set; } = null;

        public bool? SerializeGuid { get; set; } = null;

        public bool? SerializeTitle { get; set; } = null;

        public bool? SerializeModified { get; set; } = null;

        public bool? SerializeCreated { get; set; } = null;
        public bool RemoveEmptyStringValues { get; set; } = false;

        public bool RemoveBoolFalseValues { get; set; } = false;

        public bool RemoveNullValues { get; set; } = false;

        public bool RemoveZeroValues { get; set; } = false;

        #region Metadata & Relationships

        public MetadataForSerialization SerializeMetadataFor { get; set; } = null;

        public ISubEntitySerialization SerializeMetadata { get; set; } = null;

        public ISubEntitySerialization SerializeRelationships { get; set; } = null;

        #endregion
    }
}
