﻿using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// An entity should be able to specify if some properties should not be included
    /// </summary>
    /// <remarks>
    /// * Introduced v11.13 in a slightly different implementation
    /// * Enhanced as a standalone decorator in 12.05
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("Just fyi")]
    public class EntitySerializationDecorator: IDecorator<IEntity>, IEntityIdSerialization
    {
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