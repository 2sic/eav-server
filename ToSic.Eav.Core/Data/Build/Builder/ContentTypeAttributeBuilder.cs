using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build
{
    /// <summary>
    /// Builder to create / clone <see cref="IContentTypeAttribute"/> definitions.
    /// </summary>
    public class ContentTypeAttributeBuilder: ServiceBase
    {
        public ContentTypeAttributeBuilder(): base("Eav.CtAtBl")
        {
        }

        public ContentTypeAttribute Create(
            int appId,
            string name,
            ValueTypes type,
            bool isTitle,
            int id = default,
            int sortOrder = default,
            IMetadataOf metadata = default,
            List<IEntity> metadataItems = default,
            Func<IHasMetadataSource> metaSourceFinder = null)
        {
            metadata = metadata ?? new MetadataOf<int>(targetType: (int)TargetTypes.Attribute, key: id,
                    title: $"{name} ({type})", items: metadataItems,  deferredSource: metaSourceFinder);

            return new ContentTypeAttribute(appId: appId, name: name, type: type, isTitle: isTitle,
                attributeId: id, sortOrder: sortOrder, metadata: metadata);
        }



        public IContentTypeAttribute CreateFrom(
            IContentTypeAttribute original,
            int? appId = default,
            string name = default,
            ValueTypes? type = default,
            bool? isTitle = default,
            int? id = default,
            int? sortOrder = default,
            IMetadataOf metadata = default,
            List<IEntity> metadataItems = default,
            Func<IHasMetadataSource> metaSourceFinder = null
        )
        {
            // Prepare parts which we also need for new Metadata Creation
            name = name ?? original.Name;
            id = id ?? original.AttributeId;
            var realType = type ?? original.Type;
            //var metadataSpecs = ((IMetadataInternals)original.Metadata).GetCloneSpecs();
            metadata = metadata ??
                       EntityPartsBuilder.CloneMetadataFunc<int>(original.Metadata, items: metadataItems,
                           deferredSource: metaSourceFinder)(id.Value, $"{name} ({realType})");

            return Create(
                appId: appId ?? original.AppId,
                name: name,
                type: realType,
                isTitle: isTitle ?? original.IsTitle,
                id: id.Value,
                sortOrder: sortOrder ?? original.SortOrder,
                metadata: metadata
                //metadataItems: metadataItems ?? metadataSpecs.list,
                //metaSourceFinder: metaSourceFinder ?? metadataSpecs.deferredSource
            );
        }

        // 2023-02-27 2dm - was originally used by XML import, but after adding it was overwritten with "Use" so I believe it didn't have any effect.
        // Keep till ca. #2023q3 just in case we have surprises and need to undo something
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

        //    return _factory.Entity.Create(appId: appId, guid: Guid.Empty, contentType: _globalApp.GetContentType(AttributeMetadata.TypeGeneral), rawValues: valDic);
        //}
    }
}
