using System;
using System.Collections.Generic;
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
        internal Entity(int appId, int entityId,
            IContentType contentType,
            EntityPartsBuilder partsBuilder,
            bool useLightMode,
            Dictionary<string, object> values = default,
            Dictionary<string, IAttribute> typedValues = default,
            string titleAttribute = null,
            DateTime? created = null, DateTime? modified = null,
            int repositoryId = default,
            Guid? guid = default,
            string owner = default,
            int version = default,
            bool isPublished = true,
            ITarget metadataFor = default)
            : base(appId, entityId, guid, contentType, partsBuilder, values, titleAttribute, created: created, modified: modified, owner: owner, metadataFor: metadataFor)
        {
            IsLight = useLightMode;
            Attributes = typedValues;
            RepositoryId = repositoryId;
            Version = version;
            IsPublished = isPublished;
            // (IsLight, Attributes) = MapAttributesInConstructor(values);
            RepositoryId = entityId;
        }

        //private (bool isLight, Dictionary<string, IAttribute> attributes) MapAttributesInConstructor(Dictionary<string, object> values)
        //{
        //    // If all values are IAttributes, then it should be converted to a real IEntity
        //    if (values.All(x => x.Value is IAttribute))
        //    {
        //        var extendedAttribs = values
        //            .ToDictionary(x => x.Key, x => x.Value as IAttribute, InvariantCultureIgnoreCase);
        //        return (false, extendedAttribs);
        //    }

        //    // Otherwise it's a light IEntity, make sure this is known
        //    return (true, AttribBuilder.GetStatic().ConvertToInvariantDic(AttributesLight));
        //}

        #region CanBeEntity

        IEntity ICanBeEntity.Entity => this;

        #endregion
    }
}
