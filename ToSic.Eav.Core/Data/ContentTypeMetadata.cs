using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    public class ContentTypeMetadata : MetadataOf<string>
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

        //public IEnumerable<IEntity> Permissions => base.Permissions;
            //.Where(md => md.Type.StaticName == Constants.PermissionTypeName);


        protected override void LoadFromProvider()
        {
            // get the string based metadata
            base.LoadFromProvider();

            // check if it uses a guid, otherwise leave here
            if (!Guid.TryParse(Key, out var ctGuid)) return;

            // add the guid metadata on entity if it has a real guid
            var additional = GetMetadataProvider()?.GetMetadata(Constants.MetadataForEntity, ctGuid)
                                 .ToList()
                             ?? new List<IEntity>();
            Use(AllEntities.Concat(additional).ToList());
        }
    }
}
