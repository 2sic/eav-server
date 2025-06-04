using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.Metadata.Sys;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbContentType
{
    public void AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet, bool alwaysShareConfig)
    {
        var ct = GetTypeByStaticName(staticName) 
                 ?? Create(scope, usesConfigurationOfOtherSet, alwaysShareConfig);

        ct.Name = name;
        ct.Scope = scope;
        ct.TransCreatedId = DbContext.Versioning.GetTransactionId();

        // save first, to ensure it has an Id
        DbContext.SqlDb.SaveChanges();
    }

    private TsDynDataContentType Create(string scope, int? usesConfigurationOfOtherSet, bool alwaysShareConfig)
    {
        var ct = new TsDynDataContentType
        {
            AppId = DbContext.AppId,
            StaticName = Guid.NewGuid().ToString(),
            Scope = scope == "" ? null : scope,
            InheritContentTypeId = usesConfigurationOfOtherSet,
            IsGlobal = alwaysShareConfig
        };
        DbContext.SqlDb.Add(ct);
        return ct;
    }


    public void Delete(string staticName)
    {
        var setToDelete = GetTypeByStaticName(staticName);
        setToDelete.TransDeletedId = DbContext.Versioning.GetTransactionId();
        DbContext.SqlDb.SaveChanges();
    }


    private int? GetOrCreateContentType(ContentType contentType)
    {
        var destinationSet = DbContext.AttribSet.GetDbContentType(DbContext.AppId, contentType.NameId, alsoCheckNiceName: false);

        // add new Content-Type, do basic configuration if possible, then save
        if (destinationSet == null)
            destinationSet = DbContext.AttribSet.PrepareDbAttribSet(contentType.Name, contentType.NameId, contentType.Scope, false, null);

        // to use existing Content-Type, do some minimal conflict-checking
        else
        {
            DbContext.ImportLogToBeRefactored.Add(new($"Content-Type already exists{contentType.NameId}|{contentType.Name}", Message.MessageTypes.Information));
            if (destinationSet.InheritContentTypeId.HasValue)
            {
                DbContext.ImportLogToBeRefactored.Add(new("Not allowed to import/extend an Content-Type which uses Configuration of another Content-Type: " + contentType.NameId, Message.MessageTypes.Error));
                return null;
            }
        }

        // If a "Ghost"-content type is specified, try to assign that
        if (!string.IsNullOrEmpty(contentType.OnSaveUseParentStaticName))
        {
            var ghostParentId = FindGhostParentIdOrLogWarnings(contentType.OnSaveUseParentStaticName);
            if (ghostParentId == 0) return null;
            destinationSet.InheritContentTypeId = ghostParentId;
        }

        destinationSet.IsGlobal = contentType.AlwaysShareConfiguration;
        DbContext.SqlDb.SaveChanges();

        return destinationSet.ContentTypeId;
    }



    internal void ExtendSaveContentTypes(List<IContentType> contentTypes, SaveOptions saveOptions)
        => DbContext.Relationships.DoWhileQueueingRelationships(()
            => contentTypes.Cast<ContentType>()
                .ToList()
                .ForEach(ct => ExtendSaveContentTypes(ct, saveOptions))
        );

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
            if (newAtt.Metadata != null)
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
    private void SaveAttributeMetadata(int attributeId, IMetadataOf metadata, SaveOptions saveOptions)
    {
        // Verify AttributeId before we continue
        if (attributeId is 0 or < 0) // < 0 is ef-core temp id
            throw new($"trying to add metadata to attribute {attributeId} but attribute isn't saved yet");
            
        var entities = new List<IEntity>();
        // if possible, try to get the complete list which is usually hidden in IMetadataOfItem
        var sourceList = (metadata as IMetadataInternals)?.AllWithHidden as IEnumerable<IEntity> 
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
        DbContext.Save(entities, saveOptions); // don't use the standard save options, as this is attributes only
    }
        
    /// <summary>
    /// Save additional entities describing the attribute
    /// </summary>
    /// <param name="attributeId"></param>
    /// <param name="metadata"></param>
    /// <param name="saveOptions"></param>
    private void SaveTypeMetadata(string staticName, IMetadataOf metadata, SaveOptions saveOptions)
    {
        // Verify AttributeId before we continue
        if (string.IsNullOrEmpty(staticName)) //  attributeId == 0 || attributeId < 0) // < 0 is ef-core temp id
            throw new($"trying to add metadata to content-type {staticName} but name is useless");
            
        var entities = new List<IEntity>();
        // if possible, try to get the complete list which is usually hidden in IMetadataOfItem
        var sourceList = (metadata as IMetadataInternals)?.AllWithHidden as IEnumerable<IEntity> 
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
            md.KeyString = staticName;
            entities.Add(entity);
        }
        DbContext.Save(entities, saveOptions); // don't use the standard save options, as this is attributes only
    }

}