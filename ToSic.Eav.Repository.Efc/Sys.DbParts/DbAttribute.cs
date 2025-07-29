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
    {
        DbContext.DoAndSaveWithoutChangeDetection(() =>
        {
            // unset other Attributes with isTitle=true
            var all = DbContext.SqlDb.TsDynDataAttributes
                .AsNoTracking()
                .Where(s => s.ContentTypeId == contentTypeId)
                .ToListOpt();

            foreach (var attribute in all)
            {
                // check the one we want to set as title
                if (attribute.AttributeId == attributeId)
                    attribute.IsTitle = true;
                else if (!attribute.IsTitle)
                    continue; // skip if it was not set as title, to avoid unnecessary updates
                else
                    attribute.IsTitle = false;
                DbContext.SqlDb.Update(attribute);
            }
        });
    }

    internal int GetOrCreateAttributeDefinition(int contentTypeId, ContentTypeAttribute newAtt)
    {
        int destAttribId;
        if (!AttributeExistsInSet(contentTypeId, newAtt.Name))
        {
            // try to add new Attribute
            destAttribId = AppendToEndAndSave(contentTypeId, newAtt);
        }
        else
        {
            DbContext.ImportLogToBeRefactored.Add(new("Attribute already exists" + newAtt.Name, Message.MessageTypes.Information) );
            destAttribId = GetAttribute(contentTypeId, name: newAtt.Name).AttributeId;
        }
        return destAttribId;
    }


    private TsDynDataAttribute GetAttribute(int contentTypeId, int attributeId = 0, string? name = null)
    {
        try
        {
            var root = DbContext.SqlDb.TsDynDataAttributes.AsNoTracking();
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

        // ensure that it's in the set
        var attr = DbContext.SqlDb.TsDynDataAttributes
            .AsNoTracking()
            .Single(a => a.AttributeId == attributeId && a.ContentTypeId == contentTypeId);
        
        DbContext.DoAndSaveWithoutChangeDetection(() =>
        {
            attr.StaticName = newName;
            DbContext.SqlDb.Update(attr);
        });
    }

    /// <summary>
    /// Append a new Attribute to a Content-Type
    /// </summary>
    private int AppendToEndAndSave(int contentTypeId, IContentTypeAttribute contentTypeAttribute)
    {
        var maxIndex = DbContext.SqlDb.TsDynDataAttributes
            .Where(a => a.ContentTypeId == contentTypeId)
            .ToList() // important because it otherwise has problems with the next step...
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

        var contentType = DbContext.ContentTypes.TryGetDbContentTypeUntracked(DbContext.AppId, contentTypeId)
            ?? throw new($"Can't find {contentTypeId} in DB.");

        if (!AttributeNames.StaticNameValidation.IsMatch(nameId))
            throw new($"Attribute static name \"{nameId}\" is invalid. {AttributeNames.StaticNameErrorMessage}");

        // Prevent Duplicate Name
        if (AttributeExistsInSet(contentType.ContentTypeId, nameId))
            throw new ArgumentException($@"An Attribute with the static name {nameId} already exists", nameof(nameId));

        var newAttribute = new TsDynDataAttribute
        {
            Type = type,
            StaticName = nameId,
            TransCreatedId = DbContext.Versioning.GetTransactionId(),
            Guid = contentTypeAttribute.Guid,
            SysSettings = sysSettings,
            ContentTypeId = contentType.ContentTypeId,
            SortOrder = sortOrder,
            IsTitle = isTitle
        };

        // Set Attribute as Title if there's no title field in this set
        if (!contentType.TsDynDataAttributes.Any(a => a.IsTitle))
            newAttribute.IsTitle = true;


        DbContext.DoAndSaveWithoutChangeDetection(() =>
        {
            DbContext.SqlDb.Add(newAttribute);

            // If it's not a title, then we don't have to unset any old title fields
            if (!isTitle)
                return;

            // unset old Title Fields
            var oldTitleFields = contentType
                .TsDynDataAttributes
                .Where(a => a.IsTitle && a.StaticName != nameId)
                .ToListOpt();

            foreach (var titleField in oldTitleFields)
                if (titleField.IsTitle) // only change if it was set as title, to avoid unnecessary updates
                {
                    titleField.IsTitle = false;
                    DbContext.SqlDb.Update(titleField);
                }

        });
        return newAttribute.AttributeId;
    }
        


    public bool RemoveAttributeAndAllValuesAndSave(int attributeId)
    {
        DbContext.DoInTransaction(() => DbContext.DoAndSaveWithoutChangeDetection(() =>
        {
            // Remove values and valueDimensions of this attribute
            var values = DbContext.SqlDb.TsDynDataValues
                .AsNoTracking()
                .Include(v => v.TsDynDataValueDimensions)
                .Where(a => a.AttributeId == attributeId).ToList();

            values.ForEach(v =>
            {
                v.TsDynDataValueDimensions.ToList().ForEach(vd => DbContext.SqlDb.Remove(vd));
                DbContext.SqlDb.Remove(v);
            });

            var attr = DbContext.SqlDb.TsDynDataAttributes
                .AsNoTracking()
                .FirstOrDefault(a => a.AttributeId == attributeId);

            if (attr != null)
                DbContext.SqlDb.Remove(attr);

            // TODO: also consider removing metadata and formulas

        }));
        return true;
    }



}