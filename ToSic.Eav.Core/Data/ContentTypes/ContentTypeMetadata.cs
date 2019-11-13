using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Provides metadata for a content type. This can be very special, because ContentTypes can be shared (ghosts),
    /// in which case the metadata must be retrieved from another "remote" location (where the original is defined). 
    /// </summary>
    [PublicApi]
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

        /// <summary>
        /// Description of this content-type
        /// </summary>
        public IEntity Description => this
            .FirstOrDefault(md => md.Type.StaticName == Constants.ContentTypeTypeName);

        /// <summary>
        /// Load / initialize - needed when building the cache.
        /// Must usually be called a bit later, because the data is initialized from a cache, which in case of ghosts may be loaded a bit later.
        /// </summary>
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
            Use(AllWithHidden.Concat(additional).ToList());
        }
    }
}
