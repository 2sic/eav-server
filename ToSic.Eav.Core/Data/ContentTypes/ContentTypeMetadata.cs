﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Provides metadata for a content type. This can be very special, because ContentTypes can be shared (ghosts),
    /// in which case the metadata must be retrieved from another "remote" location (where the original is defined). 
    /// </summary>
    [PrivateApi("2021-09-30 hidden now, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi")]
    public class ContentTypeMetadata : MetadataOf<string>
    {

        /// <summary>
        /// Used in cases where the metadata-provider is already known
        /// </summary>
        /// <param name="typeId">type id / static-name</param>
        /// <param name="metaSourceFinder">remote / deferred metadata provider</param>
        public ContentTypeMetadata(string typeId, Func<IHasMetadataSource> metaSourceFinder, string targetIdentifier)
            : base((int)TargetTypes.ContentType, typeId, metaSourceFinder, targetIdentifier)
        { }

        /// <summary>
        /// Description <see cref="IEntity"/> metadata of this content-type.
        /// </summary>
        public IEntity Description => this
            .FirstOrDefault(md => md.Type.StaticName == ContentTypes.ContentTypeTypeName);

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
            // this is kind of wrong, because it should use the type MetadataForContentType
            // but this slipped in a long time ago, and we cannot change it any more
            var additional = GetMetadataSource()?.GetMetadata((int)TargetTypes.Entity, ctGuid)
                                 .ToList()
                             ?? new List<IEntity>();
            Use(AllWithHidden.Concat(additional).ToList());
        }
    }
}
