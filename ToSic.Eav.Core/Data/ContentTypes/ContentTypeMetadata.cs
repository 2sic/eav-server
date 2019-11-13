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

        #region constructors

        /// <summary>
        /// Used in cases where the metadata-provider is already known
        /// </summary>
        /// <param name="typeId">type id / static-name</param>
        /// <param name="metaProvider">remote / deferred metadata provider</param>
        public ContentTypeMetadata(string typeId, IDeferredEntitiesList metaProvider)
            : base(Constants.MetadataForContentType, typeId, metaProvider)
        { }

        /// <summary>
        /// Used in cases where the metadata provider isn't known, but the IDs are known
        /// </summary>
        /// <param name="typeId">type id / static-name</param>
        /// <param name="sourceZoneId">the zone which has the metadata</param>
        /// <param name="sourceAppId">the app which has the metadata</param>
        public ContentTypeMetadata(string typeId, int sourceZoneId, int sourceAppId)
            : base(Constants.MetadataForContentType, typeId, sourceZoneId, sourceAppId)
        { }

        #endregion

        /// <summary>
        /// Description <see cref="IEntity"/> metadata of this content-type.
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
