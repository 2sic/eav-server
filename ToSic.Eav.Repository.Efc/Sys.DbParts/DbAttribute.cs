using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal partial class DbAttribute(DbStorage.DbStorage db) : DbPartBase(db, "Db.AttDef")
{
    private JsonSerializer Serializer { get; } = db.JsonSerializerGenerator.New();

    /// <summary>
    /// Set an Attribute as Title on a Content-Type
    /// </summary>
    public void SetTitleAttribute(int attributeId, int contentTypeId)
        => DbStore.DoAndSaveTracked(() =>
        {
            // unset other Attributes with isTitle=true
            var all = DbStore.SqlDb.TsDynDataAttributes
                .Where(s => s.ContentTypeId == contentTypeId)
                .ToListOpt();

            foreach (var attribute in all)
            {
                // check the one we want to set as title
                if (attribute.AttributeId == attributeId)
                    attribute.IsTitle = true;
                // Check others which were previously titles
                else if (attribute.IsTitle)
                    attribute.IsTitle = false;
            }
        });

    internal int GetOrCreateAttributeDefinition(int contentTypeId, IContentTypeAttribute newAtt)
    {
        // try to add new Attribute
        if (!AttributeExistsInSet(contentTypeId, newAtt.Name))
            return AppendToEndAndSave(contentTypeId, newAtt);

        DbStore.ImportLogToBeRefactored.Add(new("Attribute already exists" + newAtt.Name, Message.MessageTypes.Information) );
        return GetAttributeUntracked(contentTypeId, name: newAtt.Name).AttributeId;
    }


    private TsDynDataAttribute GetAttributeUntracked(int contentTypeId, int attributeId = 0, string? name = null)
    {
        try
        {
            var root = DbStore.SqlDb.TsDynDataAttributes.AsNoTracking();
            return attributeId != 0
                ? root.Single(a =>a.ContentTypeId == contentTypeId && a.AttributeId == attributeId)
                : root.Single(a => a.ContentTypeId == contentTypeId && a.StaticName == name);
        }
        catch (Exception ex)
        {
            throw new("error getting attribute - content-type/setid: " + contentTypeId + "; optional attributeId: " + attributeId + "; optional name: " + name, ex);
        }
    }

    /// <summary>
    /// Set an Attribute as Title on a Content-Type
    /// </summary>
    public void RenameAttribute(int attributeId, int contentTypeId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new("can't rename to something empty");

        DbStore.DoAndSaveTracked(() =>
        {
            // ensure that it's in the set
            var attr = DbStore.SqlDb.TsDynDataAttributes
                .Single(a => a.AttributeId == attributeId && a.ContentTypeId == contentTypeId);

            attr.StaticName = newName;
        });
    }

    /// <summary>
    /// Append a new Attribute to a Content-Type
    /// </summary>
    private int AppendToEndAndSave(int contentTypeId, IContentTypeAttribute contentTypeAttribute)
    {
        var maxIndex = DbStore.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .Where(a => a.ContentTypeId == contentTypeId)
            .ToListOpt() // important because it otherwise has problems with the next step...
            .Max(s => (int?) s.SortOrder);

        return AddAttributeAndSave(contentTypeId, contentTypeAttribute, maxIndex + 1 ?? 0);
    }

    /// <summary>
    /// Append a new Attribute to a Content-Type
    /// </summary>
    public int AddAttributeAndSave(int contentTypeId, IContentTypeAttribute contentTypeAttribute, int? newSortOrder = default)
    {
        var nameId = contentTypeAttribute.Name;
        var type = contentTypeAttribute.Type.ToString();
        var isTitle = contentTypeAttribute.IsTitle;
        var sortOrder = newSortOrder ?? contentTypeAttribute.SortOrder;
        var sysSettings = Serializer.Serialize(contentTypeAttribute.SysSettings);

        var contentType = DbStore.ContentTypes
                              .GetDbContentTypeWithAttributesTracked(DbStore.AppId)
                              .SingleOrDefault(a => a.ContentTypeId == contentTypeId)
                          ?? throw new($"Can't find {contentTypeId} in DB.");

        if (!AttributeNames.StaticNameValidation.IsMatch(nameId))
            throw new($"Attribute static name \"{nameId}\" is invalid. {AttributeNames.StaticNameErrorMessage}");

        // Prevent Duplicate Name
        if (AttributeExistsInSet(contentType.ContentTypeId, nameId))
            throw new ArgumentException($@"An Attribute with the static name {nameId} already exists", nameof(nameId));

        // Set Attribute as Title if there's no title field in this set
        if (DbStore.ProcessOptions.TypeAttributeAutoSetTitle)
            if (!contentType.TsDynDataAttributes.Any(a => a.IsTitle))
                isTitle = true;

        // Build...
        var newAttribute = new TsDynDataAttribute
        {
            Type = type,
            StaticName = nameId,
            TransCreatedId = DbStore.Versioning.GetTransactionId(),
            Guid = contentTypeAttribute.Guid,
            SysSettings = sysSettings,
            ContentTypeId = contentType.ContentTypeId,
            SortOrder = sortOrder,
            IsTitle = isTitle
        };

        DbStore.DoAndSaveTracked(() =>
        {
            DbStore.SqlDb.Add(newAttribute);

            // If it's not a title, then we don't have to unset any old title fields, so exit early
            if (!isTitle || !DbStore.ProcessOptions.TypeAttributeAutoCorrectTitle)
                return;

            // unset old Title Fields...
            var oldTitleFields = contentType
                .TsDynDataAttributes
                .Where(a => a.IsTitle && a.StaticName != nameId)
                .ToListOpt();

            // ...only change if it was set as title, to avoid unnecessary updates
            foreach (var titleField in oldTitleFields)
                if (titleField.IsTitle)
                    titleField.IsTitle = false;

        });
        return newAttribute.AttributeId;
    }
        


    public bool RemoveAttributeAndAllValuesAndSave(int attributeId)
    {
        DbStore.DoInTransaction(() => DbStore.DoAndSaveWithoutChangeDetection(() =>
        {
            // Remove values and valueDimensions of this attribute
            var values = DbStore.SqlDb.TsDynDataValues
                .AsNoTracking()
                .Include(v => v.TsDynDataValueDimensions)
                .Where(a => a.AttributeId == attributeId).ToList();

            foreach (var v in values)
                DbStore.SqlDb.RemoveRange(v.TsDynDataValueDimensions);

            var attr = DbStore.SqlDb.TsDynDataAttributes
                .AsNoTracking()
                .FirstOrDefault(a => a.AttributeId == attributeId);

            if (attr != null)
                DbStore.SqlDb.Remove(attr);

            // TODO: also consider removing metadata and formulas

        }));
        return true;
    }

}