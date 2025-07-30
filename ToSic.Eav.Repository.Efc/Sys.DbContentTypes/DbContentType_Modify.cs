using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Repository.Efc.Sys.DbContentTypes;

partial class DbContentType
{
    public void AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet, bool alwaysShareConfig)
        => DbContext.DoAndSaveTracked(() =>
        {
            var ct = TryGetTypeByStaticTracked(staticName);

            // If exists, do update
            if (ct != null)
            {
                ct.Name = name;
                ct.Scope = scope;
                ct.TransCreatedId = DbContext.Versioning.GetTransactionId();
                return;
            }

            // If not exists, create new
            DbContext.SqlDb.Add(PrepareNew(name, scope, usesConfigurationOfOtherSet, alwaysShareConfig));
        });

    private TsDynDataContentType PrepareNew(string name, string scope, int? usesConfigurationOfOtherSet, bool alwaysShareConfig)
        => new()
        {
            AppId = DbContext.AppId,
            Name = name,
            StaticName = Guid.NewGuid().ToString(),
            Scope = scope == "" ? null : scope,
            InheritContentTypeId = usesConfigurationOfOtherSet,
            IsGlobal = alwaysShareConfig,
            TransCreatedId = DbContext.Versioning.GetTransactionId(),
        };


    public void Delete(string nameId)
        => DbContext.DoAndSaveTracked(() =>
        {
            var setToDelete = TryGetTypeByStaticTracked(nameId)
                              ?? throw new ArgumentException($@"Tried to delete but can't find {nameId}",
                                  nameof(nameId));
            setToDelete.TransDeletedId = DbContext.Versioning.GetTransactionId();
        });


    private int? GetOrCreateContentType(ContentType contentType)
    {
        var newType = DbContext.ContentTypes
            .GetDbContentTypeWithAttributesUntracked(DbContext.AppId)
            .SingleOrDefault(a => a.StaticName == contentType.NameId);

        //var isUpdate = newType != null;
        
        // to use existing Content-Type, do some minimal conflict-checking
        if (newType != null)
        {
            // TODO: VERY UNCLEAR why we are adding special messages on a Get-situation...
            // 2025-07-30 disable all these messages and null-return, I believe this is very old stuff which doesn't make sense anymore
            //DbContext.ImportLogToBeRefactored.Add(new($"Content-Type already exists{contentType.NameId}|{contentType.Name}", Message.MessageTypes.Information));
            //if (newType.InheritContentTypeId.HasValue)
            //{
            //    DbContext.ImportLogToBeRefactored.Add(new("Not allowed to import/extend an Content-Type which uses Configuration of another Content-Type: " +
            //                                              contentType.NameId, Message.MessageTypes.Error));
            //    return null;
            //}

            return newType.ContentTypeId;
        }

        // add new Content-Type, do basic configuration if possible, then save
        newType = DbContext.ContentTypes.PrepareDbContentType(contentType.Name, contentType.NameId,
                      contentType.Scope, false, null)
                  ?? throw new($"Can't create content type {contentType.Name}/{contentType.NameId}");

        #region Moved Here 2025-07-30 as it changes something to a ghost / global type, which I believe can only be done on creation; previously it could have also been an edit...

        // If a "Ghost"-content type is specified, try to assign that
        if (!string.IsNullOrEmpty(contentType.OnSaveUseParentStaticName))
        {
            var ghostParentId = FindGhostParentIdOrLogWarnings(contentType.OnSaveUseParentStaticName!);
            if (ghostParentId == 0)
                return null;
            newType.InheritContentTypeId = ghostParentId;
        }

        newType.IsGlobal = contentType.AlwaysShareConfiguration;

        #endregion

        DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Add(newType));

        return newType.ContentTypeId;
    }



    internal void ExtendSaveContentTypes(List<IContentType> contentTypes, SaveOptions saveOptions)
        => DbContext.Relationships.DoWhileQueueingRelationshipsUntracked(() =>
        {
            var typed = contentTypes.Cast<ContentType>().ToList();
            foreach (var ct in typed)
                ExtendSaveContentTypes(ct, saveOptions);
        });

    /// <summary>
    /// Import a Content-Type with all Attributes and AttributeMetaData
    /// </summary>
    private void ExtendSaveContentTypes(ContentType contentType, SaveOptions saveOptions)
    {
        // initialize destinationSet - create or test existing if ok
        var foundSet = GetOrCreateContentType(contentType);
        if (foundSet == null) // something went wrong, skip this import
            return;
        var contentTypeId = foundSet.Value;

        // 2dm 2021-04-08 Metadata wasn't saved before v11.14+
        if (contentType.Metadata.Any())
            SaveTypeMetadata(contentType.NameId, contentType.Metadata, saveOptions);

        // append all Attributes
        foreach (var newAtt in contentType.Attributes.Cast<ContentTypeAttribute>())
        {
            var destAttribId = DbContext.Attributes.GetOrCreateAttributeDefinition(contentTypeId, newAtt);

            // save additional entities containing AttributeMetaData for this attribute
            if (newAtt.Metadata != null!)
                SaveAttributeMetadata(destAttribId, newAtt.Metadata, saveOptions);
        }

        // optionally re-order the attributes if specified in import
        if (contentType.OnSaveSortAttributes)
            SortAttributes(contentTypeId, contentType);
    }


    /// <summary>
    /// Save additional entities describing the attribute
    /// </summary>
    /// <param name="attributeId"></param>
    /// <param name="metadata"></param>
    /// <param name="saveOptions"></param>
    private void SaveAttributeMetadata(int attributeId, IMetadata metadata, SaveOptions saveOptions)
    {
        // Verify AttributeId before we continue
        if (attributeId is 0 or < 0) // < 0 is ef-core temp id
            throw new($"trying to add metadata to attribute {attributeId} but attribute isn't saved yet");
            
        var entities = new List<IEntity>();
        // if possible, try to get the complete list which is usually hidden in IMetadataOfItem
        var sourceList = (metadata as IMetadataInternals)
                         ?.AllWithHidden as IEnumerable<IEntity>
                         ?? metadata;
        foreach (var entity in sourceList)
        {
            // it's important that we set the target properties
            // as the data may simply have been attached and still carry wrong owner-markers
            // TODO: this should be ensured at the merge-level 
            // MergeContentTypeUpdateWithExisting(AppState appState, IContentType contentType)
            var md = (Target)entity.MetadataFor;
            // Set type / key
            md.TargetType = (int)TargetTypes.Attribute;
            md.KeyNumber = attributeId;
            entities.Add(entity);
        }

        var withOptions = saveOptions.AddToAll(entities);
        DbContext.Save(withOptions); // don't use the standard save options, as this is attributes only
    }
        
    /// <summary>
    /// Save additional entities describing the attribute
    /// </summary>
    /// <param name="nameId"></param>
    /// <param name="metadata"></param>
    /// <param name="saveOptions"></param>
    private void SaveTypeMetadata(string nameId, IMetadata metadata, SaveOptions saveOptions)
    {
        // Verify AttributeId before we continue
        if (string.IsNullOrEmpty(nameId)) //  attributeId == 0 || attributeId < 0) // < 0 is ef-core temp id
            throw new($"trying to add metadata to content-type {nameId} but name is useless");
            
        var entities = new List<IEntity>();
        // if possible, try to get the complete list which is usually hidden in IMetadataOfItem
        var sourceList = (metadata as IMetadataInternals)
                         ?.AllWithHidden as IEnumerable<IEntity> 
                         ?? metadata;
        foreach (var entity in sourceList)
        {
            // it's important that we set the target properties
            // as the data may simply have been attached and still carry wrong owner-markers
            // TODO: this should be ensured at the merge-level 
            // MergeContentTypeUpdateWithExisting(AppState appState, IContentType contentType)
            var md = (Target)entity.MetadataFor;
            // Set type / key
            md.TargetType = (int)TargetTypes.ContentType;
            md.KeyString = nameId;
            entities.Add(entity);
        }
        var withOptions = saveOptions.AddToAll(entities);
        DbContext.Save(withOptions); // don't use the standard save options, as this is attributes only
    }

}