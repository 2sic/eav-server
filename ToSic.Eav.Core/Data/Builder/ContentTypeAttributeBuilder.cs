using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public class ContentTypeAttributeBuilder: ServiceBase
    {
        public ContentTypeAttributeBuilder(): base("Eav.CtAtBl")
        {
        }

        public ContentTypeAttribute Create(
            int appId,
            string name,
            string type,
            bool isTitle,
            int attributeId = default,
            int sortOrder = default,
            IMetadataOf metadata = default,
            List<IEntity> metadataItems = default,
            Func<IHasMetadataSource> metaSourceFinder = null)
        {
            metadata = metadata ?? new MetadataOf<int>(targetType: (int)TargetTypes.Attribute, key: attributeId,
                    title: name + " (" + type + ")", items: metadataItems,  deferredSource: metaSourceFinder);

            return new ContentTypeAttribute(appId: appId, name: name, type: type, isTitle: isTitle,
                attributeId: attributeId, sortOrder: sortOrder, metadata: metadata);
        }

        public IContentTypeAttribute Clone(IContentTypeAttribute original,
            int? appId = default,
            string name = default,
            string type = default,
            bool? isTitle = default,
            int? attributeId = default,
            int? sortOrder = default,
            IMetadataOf metadata = default,
            List<IEntity> metadataItems = default,
            Func<IHasMetadataSource> metaSourceFinder = null
        )
        {
            var metadataSpecs = ((IMetadataInternals)original.Metadata).GetCloneSpecs();

            return Create(
                appId: appId ?? original.AppId,
                name: name ?? original.Name,
                type: type ?? original.Type,
                isTitle: isTitle ?? original.IsTitle,
                attributeId: attributeId ?? original.AttributeId,
                sortOrder: sortOrder ?? original.SortOrder,
                metadata: metadata,
                metadataItems: metadataSpecs.list,
                metaSourceFinder: metadataSpecs.deferredSource
            );
        }

        // 2023-02-27 2dm - was originally used by XML import, but after adding it was overwritten with "Use" so I believe it didn't have any effect.
        ///// <summary>
        ///// Shortcut to get an @All Entity Describing an Attribute
        ///// </summary>
        //public Entity GenerateAttributeMetadata(int appId, string name, string notes, bool? visibleInEditUi, string defaultValue, string inputType)
        //{
        //    var valDic = new Dictionary<string, object>();
        //    if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
        //    if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
        //    if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
        //    if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);
        //    if (!string.IsNullOrEmpty(inputType)) valDic.Add(AttributeMetadata.GeneralFieldInputType, inputType);

        //    return _builder.Entity.Create(appId: appId, guid: Guid.Empty, contentType: _globalApp.GetContentType(AttributeMetadata.TypeGeneral), rawValues: valDic);
        //}
    }
}
