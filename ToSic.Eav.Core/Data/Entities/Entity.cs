﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Lib.Documentation;
using static System.StringComparer;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// A basic unit / item of data. Has many <see cref="IAttribute{T}"/>s which then contains <see cref="IValue{T}"/>s which are multi-language. 
    /// </summary>
    [PrivateApi("2021-09-30 hidden, previously InternalApi_DoNotUse_MayChangeWithoutNotice this is just fyi, always use IEntity")]

    public partial class Entity: EntityLight, IEntity
    {
        /// <inheritdoc />
        /// <summary>
        /// Special constructor for entity-builders.
        /// Goal is that this results in a final, full IEntity which will then be immutable
        /// This is still WIP @2dm 2023-02-19
        ///
        /// For now we're including parameters which should not have a public setter
        /// </summary>
        [PrivateApi]
        internal Entity(Dictionary<string, IAttribute> attributes)
        {
            Attributes = (attributes ?? new Dictionary<string, IAttribute>()).ToInvariant();
        }

        /// <summary>
        /// Special constructor for importing-new/creating-external entities without a known content-type
        /// </summary>
        [PrivateApi]
        public Entity(int appId, int entityId, Guid entityGuid, string contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null, string owner = null) 
            : base(appId, entityId, entityGuid, new ContentType(appId, contentType), values, titleAttribute, modified: modified, owner: owner)
        {
            (IsLight, Attributes) = MapAttributesInConstructor(values);
            RepositoryId = entityId;
        }

        [PrivateApi]
        public Entity(int appId, int entityId, IContentType contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? created = null, DateTime? modified = null, Guid? guid = null, string owner = null) 
            : base(appId, entityId, guid, contentType, values, titleAttribute, created: created, modified: modified, owner: owner)
        {
            (IsLight, Attributes) = MapAttributesInConstructor(values);
            RepositoryId = entityId;
        }

        private (bool isLight, Dictionary<string, IAttribute> attributes) MapAttributesInConstructor(Dictionary<string, object> values)
        {
            // If all values are IAttributes, then it should be converted to a real IEntity
            if (values.All(x => x.Value is IAttribute))
            {
                var extendedAttribs = values
                    .ToDictionary(x => x.Key, x => x.Value as IAttribute, InvariantCultureIgnoreCase);
                return (false, extendedAttribs);
            }

            // Otherwise it's a light IEntity, make sure this is known
            return (true, AttribBuilder.GetStatic().ConvertToInvariantDic(AttributesLight));
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a brand new Entity. 
        /// Mainly used for entities which are created for later saving
        /// </summary>
        [PrivateApi]
        public Entity(int appId, Guid entityGuid, IContentType contentType, Dictionary<string, object> values, string owner = null) 
            : this(appId, 0, contentType, values, guid: entityGuid, owner: owner)
        {}

        #region CanBeEntity

        IEntity ICanBeEntity.Entity => this;

        #endregion
    }
}
