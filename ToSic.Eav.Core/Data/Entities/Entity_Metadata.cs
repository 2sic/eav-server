using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        public IMetadataOf Metadata => _metadata ?? (_metadata =
            new MetadataOf<Guid>((int)TargetTypes.Entity, EntityGuid, DeferredLookupData, GetBestTitle() ?? "entity with title unknown"));
        private IMetadataOf _metadata;
        internal IHasMetadataSource DeferredLookupData = null;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions => Metadata.Permissions;
    }
}
