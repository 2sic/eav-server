﻿using System;
using System.Collections.Immutable;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A basic unit / item of data. Has many <see cref="IAttribute{T}"/>s which then contains <see cref="IValue{T}"/>s which are multi-language. 
    /// </summary>
    [PrivateApi("2021-09-30 hidden, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]

    public partial class Entity: EntityLight, IEntity
    {
        [PrivateApi]
        internal Entity(int appId,
            int entityId,
            IContentType contentType,
            EntityPartsBuilder partsBuilder,
            //bool useLightMode,
            IImmutableDictionary<string, object> rawValues,
            IImmutableDictionary<string, IAttribute> values,
            string titleFieldName,
            DateTime? created, DateTime? modified,
            int repositoryId,
            Guid? guid,
            string owner,
            int version,
            bool isPublished,
            ITarget metadataFor,
            bool placeDraftInBranch = false,
            int publishedId = default)
            : base(appId, entityId, guid, contentType, partsBuilder, rawValues: null, titleFieldName, created: created, modified: modified, owner: owner, metadataFor: metadataFor)
        {
            Attributes = values;
            RepositoryId = repositoryId;
            Version = version;
            IsPublished = isPublished;
            PlaceDraftInBranch = placeDraftInBranch;
            PublishedEntityId = publishedId;
            _getMetadataOf = partsBuilder.GetMetadataOf;
        }

        #region CanBeEntity

        IEntity ICanBeEntity.Entity => this;

        #endregion

        #region ToString to improve debugging experience

        public override string ToString() => $"{GetType()} =id:{EntityId}/{EntityGuid}";

        #endregion
    }
}
