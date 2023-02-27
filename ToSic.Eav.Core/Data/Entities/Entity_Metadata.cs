using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        //public IMetadataOf Metadata => _metadata ?? (_metadata =
        //    new MetadataOf<Guid>((int)TargetTypes.Entity, EntityGuid, GetBestTitle() ?? "entity with title unknown", appSource: DeferredLookupData));
        //private IMetadataOf _metadata;
        internal IHasMetadataSource DeferredLookupData = null;
        public IMetadataOf Metadata => _metadataOf.Get(() => _getMetadataOf(EntityGuid, GetBestTitle() ?? "entity with unknown title"));
        private readonly GetOnce<IMetadataOf> _metadataOf = new GetOnce<IMetadataOf>();
        private readonly Func<Guid, string, IMetadataOf> _getMetadataOf;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions => Metadata.Permissions;
    }
}
