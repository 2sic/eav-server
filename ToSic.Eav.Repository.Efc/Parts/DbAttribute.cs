using System.Diagnostics;
using ToSic.Eav.ImportExport.Json;

namespace ToSic.Eav.Repository.Efc.Parts;

internal partial class DbAttribute(DbDataController db) : DbPartBase(db, "Db.AttDef")
{
    private JsonSerializer Serializer { get; } = db.JsonSerializerGenerator.New();

    /// <summary>
    /// Set an Attribute as Title on an AttributeSet
    /// </summary>
    public void SetTitleAttribute(int attributeId, int contentTypeId)
    {
        GetAttribute(contentTypeId, attributeId).IsTitle = true;

        // unset other Attributes with isTitle=true
        var oldTitleAttributes = DbContext.SqlDb.TsDynDataAttributes
            .Where(s => s.ContentTypeId == contentTypeId && s.IsTitle);
        foreach (var oldTitleAttribute in oldTitleAttributes)
            oldTitleAttribute.IsTitle = false;

        DbContext.SqlDb.SaveChanges();
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
            DbContext.ImportLogToBeRefactored.Add(new(EventLogEntryType.Information, "Attribute already exists" + newAtt.Name));
            destAttribId = AttributeId(contentTypeId, newAtt.Name);
        }
        return destAttribId;
    }


    private TsDynDataAttribute GetAttribute(int contentTypeId, int attributeId = 0, string name = null)
    {
        try
        {
            return attributeId != 0
                ? DbContext.SqlDb.TsDynDataAttributes
                    .Single(a =>a.ContentTypeId == contentTypeId && a.AttributeId == attributeId)
                : DbContext.SqlDb.TsDynDataAttributes
                    .Single(a => a.ContentTypeId == contentTypeId && a.StaticName == name);
        }
        catch (Exception ex)
        {
            throw new("error getting attribute - content-type/setid: " + contentTypeId + "; optional attributeId: " + attributeId + "; optional name: " + name, ex);
        }
    }


    private int AttributeId(int setId, string staticName) => GetAttribute(setId, name: staticName).AttributeId;

    /// <summary>
    /// Set an Attribute as Title on an AttributeSet
    /// </summary>
    public void RenameAttribute(int attributeId, int contentTypeId, string newName)
    {
        if(string.IsNullOrWhiteSpace(newName))
            throw new("can't rename to something empty");

        // ensure that it's in the set
        var attr = DbContext.SqlDb.TsDynDataAttributes
            .Single(a => a.AttributeId == attributeId && a.ContentTypeId == contentTypeId);
        attr.StaticName = newName;
        DbContext.SqlDb.SaveChanges();
    }

    /// <summary>
    /// Append a new Attribute to an AttributeSet
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
    /// Append a new Attribute to an AttributeSet
    /// </summary>
    public int AddAttributeAndSave(int contentTypeId, IContentTypeAttribute contentTypeAttribute, int? newSortOrder = default)
    {
        var staticName = contentTypeAttribute.Name;
        var type = contentTypeAttribute.Type.ToString();
        var isTitle = contentTypeAttribute.IsTitle;
        var sortOrder = newSortOrder ?? contentTypeAttribute.SortOrder;
        var sysSettings = Serializer.Serialize(contentTypeAttribute.SysSettings);

        var contentType = DbContext.AttribSet.GetDbContentType(DbContext.AppId, contentTypeId);

        if (!Attributes.StaticNameValidation.IsMatch(staticName))
            throw new("Attribute static name \"" + staticName + "\" is invalid. " + Attributes.StaticNameErrorMessage);

        // Prevent Duplicate Name
        if (AttributeExistsInSet(contentType.ContentTypeId, staticName))
            throw new ArgumentException($@"An Attribute with the static name {staticName} already exists", nameof(staticName));

        var newAttribute = new TsDynDataAttribute
        {
            Type = type,
            StaticName = staticName,
            TransactionIdCreated = DbContext.Versioning.GetTransactionId(),
            Guid = contentTypeAttribute.Guid,
            SysSettings = sysSettings,
            ContentType = contentType,
            SortOrder = sortOrder,
            // AttributeGroupId = 1,
            IsTitle = isTitle
        };
        DbContext.SqlDb.Add(newAttribute);

        // Set Attribute as Title if there's no title field in this set
        if (!contentType.TsDynDataAttributes.Any(a => a.IsTitle))
            newAttribute.IsTitle = true;

        if (isTitle)
        {
            // unset old Title Fields
            var oldTitleFields = contentType.TsDynDataAttributes.Where(a => a.IsTitle && a.StaticName != staticName).ToList();
            foreach (var titleField in oldTitleFields)
                titleField.IsTitle = false;
        }

        DbContext.SqlDb.SaveChanges();
        return newAttribute.AttributeId;
    }
        


    public bool RemoveAttributeAndAllValuesAndSave(int attributeId)
    {
        DbContext.DoInTransaction(() =>
        {
            // Remove values and valueDimensions of this attribute
            var values = DbContext.SqlDb.TsDynDataValues
                .Include(v => v.TsDynDataValueDimensions)
                .Where(a => a.AttributeId == attributeId).ToList();

            values.ForEach(v =>
            {
                v.TsDynDataValueDimensions.ToList().ForEach(vd => DbContext.SqlDb.Remove(vd));
                DbContext.SqlDb.TsDynDataValues.Remove(v);
            });
            DbContext.SqlDb.SaveChanges();

            var attr = DbContext.SqlDb.TsDynDataAttributes.FirstOrDefault(a => a.AttributeId == attributeId);

            if (attr != null)
                DbContext.SqlDb.TsDynDataAttributes.Remove(attr);

            DbContext.SqlDb.SaveChanges();
        });
        return true;
    }



}