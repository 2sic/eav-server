using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    public class ContentTypeMetadata : OfMetadataOfItem<string>
    {

        #region empty constructors
        public ContentTypeMetadata(string key, IDeferredEntitiesList metaProvider)
            : base(Constants.MetadataForContentType, key, metaProvider)
        { }

        public ContentTypeMetadata(string key, int remoteZoneId, int remoteAppId)
            : base(Constants.MetadataForContentType, key, remoteZoneId, remoteAppId)
        { }

        #endregion

        public IEntity Description => this
            .FirstOrDefault(md => md.Type.StaticName == Constants.ContentTypeTypeName);

        public IEnumerable<IEntity> Permissions => this
            .Where(md => md.Type.StaticName == Constants.PermissionTypeName);


        protected override IMetadataProvider LoadFromProvider()
        {
            // get the string based metadata
            var mdProvider = base.LoadFromProvider();

            // add the guid metadata on entity if it has a real guid
            if (!Guid.TryParse(Key, out var ctGuid)) return mdProvider;

            var additional = mdProvider?.GetMetadata(Constants.MetadataForEntity, ctGuid)
                                 .ToList()
                             ?? new List<IEntity>();
            Entities = Entities.Concat(additional).ToList();
            return mdProvider;
        }
    }
}
