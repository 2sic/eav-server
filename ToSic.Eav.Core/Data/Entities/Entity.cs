using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Documentation;

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
        /// special blank constructor for entity-builders
        /// </summary>
        [PrivateApi]
        internal Entity() { }

        /// <summary>
        /// Special constructor for importing-new/creating-external entities without a known content-type
        /// </summary>
        [PrivateApi]
        public Entity(int appId, int entityId, Guid entityGuid, string contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? modified = null, string owner = null) 
            : base(appId, entityId, entityGuid, new ContentType(appId, contentType), values, titleAttribute, modified: modified, owner: owner)
        {
            MapAttributesInConstructor(values);
        }

        [PrivateApi]
        public Entity(int appId, int entityId, IContentType contentType, Dictionary<string, object> values, string titleAttribute = null, DateTime? created = null, DateTime? modified = null, Guid? guid = null, string owner = null) 
            : base(appId, entityId, guid, contentType, values, titleAttribute, created: created, modified: modified, owner: owner)
        {
            MapAttributesInConstructor(values);
        }

        private void MapAttributesInConstructor(Dictionary<string, object> values)
        {
            if (values.All(x => x.Value is IAttribute))
                Attributes = values
                    .ToDictionary(x => x.Key, x => x.Value as IAttribute);
            else
                _useLightModel = true;
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
