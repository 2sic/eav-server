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

        #region Metadata & Relationships

        public bool? SerializeMetadataFor { get; set; } = null;

        public ISubEntitySerialization SerializeMetadata { get; set; } = null;

        public ISubEntitySerialization SerializeRelationships { get; set; } = null;

        #endregion
    }
}
