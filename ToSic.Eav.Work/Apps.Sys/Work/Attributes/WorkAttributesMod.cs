using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.ContentTypes.Fields;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;
using ToSic.Eav.Serialization;
using ToSic.Sys.Capabilities.Features;
using static ToSic.Sys.Capabilities.Features.BuiltInFeatures;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkAttributesMod(
    GenWorkDb<WorkMetadata> workMetadata,
    GenWorkBasic<WorkAttributes> workAttributes,
    ContentTypeFieldAssembler fieldAssembler,
    Generator<IDataDeserializer> dataDeserializer,
    LazySvc<ISysFeaturesService> features,
    LazySvc<ContentTypeChangeActionRunner> contentTypeChangeActions)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.AttMod",
        connect: [fieldAssembler, workMetadata, workAttributes, features, dataDeserializer, contentTypeChangeActions])
{
    #region Getters which don't modify, but need the DB

    /// <summary>
    /// Get all known data types, like "String", "Number" etc. from DB.
    /// It should actually not be in the ...Mod because it doesn't modify anything, but it's here because it needs the DB.
    /// </summary>
    /// <returns></returns>
    public string[] DataTypes()
    {
        var l = Log.Fn<string[]>();
        var result = AppWorkCtx.DbStorage.Attributes.DataTypeNames();
        return l.Return(result, $"{result.Length}");
    }

    #endregion

    #region Add Field

    public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder, bool triggerPostSave = true)
    {
        var l = Log.Fn<int>($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
        var attDef = fieldAssembler.Create(
            appId: AppWorkCtx.AppId,
            name: staticName,
            type: ValueTypeHelpers.Get(type),
            isTitle: false,
            id: 0,
            sortOrder: sortOrder
        );
        var id = AddFieldToDbAndInitGeneralMetadata(contentTypeId, attDef, inputType);
        if (triggerPostSave)
            // Field definition changed => generated models may have new/removed properties.
            TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.Return(id);
    }


    /// <summary>
    /// Append a new Field to an ContentType
    /// Simple overload returning int, so it can be used from outside
    /// </summary>
    private int AddFieldToDbAndInitGeneralMetadata(int contentTypeId, IContentTypeField attDef, string inputType)
    {
        var l = Log.Fn<int>($"type:{contentTypeId}, input:{inputType}");
        var newAttribute = AppWorkCtx.DbStorage.Attributes.AddAttributeAndSave(contentTypeId, attDef);

        // set the nice name and input type, important for newly created attributes
        InitializeNameAndInputType(attDef.Name, inputType, newAttribute);

        return l.ReturnAndLog(newAttribute);
    }

    private void InitializeNameAndInputType(string staticName, string inputType, int attributeId)
    {
        var l = Log.Fn($"attrib:{attributeId}, name:{staticName}, input:{inputType}");
        // new: set the inputType - this is a bit tricky because it needs an attached entity of type @All to set the value to...
        var newValues = new Dictionary<string, object>
        {
            { "VisibleInEditUI", true },
            { "Name", staticName },
            { nameof(IFieldSettingsGeneral.InputType), inputType }
        };
        var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
        workMetadata.New(AppWorkCtx).SaveMetadata(meta, IFieldSettingsGeneral.Constants.ContentTypeName, newValues);
        l.Done();
    }

    #endregion

    #region Changes to input type, name, etc.

    public bool SetInputType(int attributeId, string inputType)
    {
        var l = Log.Fn<bool>($"attrib:{attributeId}, input:{inputType}");
        // Capture content-type before mutation because this path only receives fieldDef id.

        var attribute = AppWorkCtx.DbStorage.Attributes.GetTracked(attributeId)
                        ?? throw new ArgumentException($"Field with id {attributeId} does not exist.");
        var contentTypeId = attribute.ContentTypeId;

        var newValues = new Dictionary<string, object> { { nameof(IFieldSettingsGeneral.InputType), inputType } };

        var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
        workMetadata.New(AppWorkCtx).SaveMetadata(meta, IFieldSettingsGeneral.Constants.ContentTypeName, newValues);
        TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.ReturnTrue();
    }

    public bool Rename(int contentTypeId, int attributeId, string newName)
    {
        var l = Log.Fn<bool>($"rename fieldDef type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
        AppWorkCtx.DbStorage.Attributes.RenameAttribute(attributeId, contentTypeId, newName);
        TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.ReturnTrue();
    }

    public bool Reorder(int contentTypeId, string orderCsv)
    {
        var l = Log.Fn<bool>($"reorder type#{contentTypeId}, order:{orderCsv}");
        var sortOrderList = orderCsv.Split(',').Select(int.Parse).ToList();
        AppWorkCtx.DbStorage.ContentType.SortAttributes(contentTypeId, sortOrderList);
        TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.ReturnTrue();
    }


    public bool Delete(int contentTypeId, int attributeId)
    {
        var l = Log.Fn<bool>($"delete field type#{contentTypeId}, attrib:{attributeId}");
        var success = AppWorkCtx.DbStorage.Attributes.RemoveAttributeAndAllValuesAndSave(attributeId);
        // Trigger only when delete succeeded; failed delete should not cause code regeneration.
        if (success)
            TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.Return(success);
    }


    #endregion

    #region New Sharing Features

    public bool FieldShare(int attributeId, bool share, bool hide = false)
    {
        var l = Log.Fn<bool>($"attributeId:{attributeId}, share:{share}, hide:{hide}");
        var contentTypeId = 0;

        if (!features.Value.IsEnabled(ContentTypeFieldsReuseDefinitions.Guid))
            l.W("Setting up field share but feature is not enabled / licensed.");

        var serializer = dataDeserializer.New();
        serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

        // Update DB, and then flush the app-cache as necessary, same as any other fieldDef change
        AppWorkCtx.DbStorage.DoAndSaveTracked(() =>
        {
            // get field attributeId
            var attribute = AppWorkCtx.DbStorage.Attributes.GetTracked(attributeId)
                ?? throw new ArgumentException($"Field with id {attributeId} does not exist.");
            contentTypeId = attribute.ContentTypeId;

            // ensure GUID: update the field definition in the DB to ensure it has a GUID (but don't change if it already has one)
            if (attribute.Guid.HasValue == false)
                attribute.Guid = Guid.NewGuid();

            // update with the Share = share (hide we'll ignore for now, it's for future needs)
            attribute.SysSettings = serializer.Serialize(new()
            {
                Share = share,
            });
        });

        if (contentTypeId > 0)
            // Sharing alters effective schema behavior and should re-run generators.
            TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.ReturnTrue();
    }

    public bool FieldInherit(int attributeId, Guid inheritMetadataOf, bool triggerPostSave = true)
    {
        var l = Log.Fn<bool>($"attributeId:{attributeId}, inheritMetadataOf:{inheritMetadataOf}");
        var contentTypeId = 0;

        if (!features.Value.IsEnabled(ContentTypeFieldsReuseDefinitions.Guid))
            l.W("Setting up field share but feature is not enabled / licensed.");

        // Prepare serializer
        var serializer = dataDeserializer.New();
        serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

        // Update DB, and then flush the app-cache as necessary, same as any other fieldDef change
        AppWorkCtx.DbStorage.DoAndSaveTracked(() =>
        {
            // get field attributeId
            var attribute = AppWorkCtx.DbStorage.Attributes.GetTracked(attributeId)
                ?? throw new ArgumentException($"Field with id {attributeId} does not exist.");
            contentTypeId = attribute.ContentTypeId;

            // set InheritMetadataOf to the guid above(as string)
            attribute.SysSettings = serializer.Serialize(new()
            {
                Inherit = null,
                InheritNameOfPrimary = false,
                InheritMetadataOfPrimary = false,
                InheritMetadataOf = new() { [inheritMetadataOf] = "" },
            });
        });

        if (triggerPostSave && contentTypeId > 0)
            // Allow caller to suppress trigger when this method is part of a larger multi-step operation.
            TriggerPostSaveForContentType(GetContentType(contentTypeId));
        return l.ReturnTrue();
    }

    public bool AddInheritedField(int contentTypeId, string sourceType, Guid sourceField, string name)
    {
        var l = Log.Fn<bool>();

        if (!features.Value.IsEnabled(ContentTypeFieldsReuseDefinitions.Guid))
            l.W("Setting up field share but feature is not enabled / licensed.");

        // 1. First check that sources are correct

        // 1.1 split the fields.value by the "/" - format should be "TypeStaticNameUsuallyGuid/Field-Guid"
        // - first component should be the original content-type
        // - second the source field guid
        // - note that the content-type wouldn't be necessary, but we want to have it to prevent mistakes if for some reason the guid is duplicate
        // - verify that the source fields exist, and really belong to the content-types they claim to be from
        var fields = workAttributes.New(AppWorkCtx.AppId).GetSharedFields(attributeId: default)
            .Where(f => f.Type.NameId == sourceType && f.Field.Guid == sourceField).ToList();

        // 1.2 Find the source fields and only keep the ones that are valid
        if (fields.Count == 0)
            return l.ReturnFalse($"error: wrong sourceType {sourceType} or sourceField {sourceField}");
        if (fields.Count > 1)
            return l.ReturnFalse($"error: we have multiple: {fields.Count} duplicate shared fields with same sourceType {sourceType} and sourceField {sourceField}");

        var pairTypeWithAttribute = fields.Single();

        // 2. Create attributes

        // 2.1 find the index for adding fields
        // - get the content-type
        var contentType = AppWorkCtx.AppReader.GetContentTypeOptional(contentTypeId);
        if (contentType == null)
            return l.ReturnFalse($"error: wrong contentTypeId {contentTypeId}");
        // - make sure we have the fieldDef-count to add more fields

        // 2.2 create the attributes based on the original data
        // - name is the key in the dictionary
        // - probably just call AddField code above
        // - of course increment the start-index for each field
        var newAttributeId = AddField(contentTypeId, name,
            type: pairTypeWithAttribute.Field.Type.ToString(),
            inputType: pairTypeWithAttribute.Field.InputType,
            sortOrder: contentType.Attributes.Count() + 1,
            triggerPostSave: false);

        // 3. Configure inherit
        FieldInherit(newAttributeId, inheritMetadataOf: sourceField, triggerPostSave: false);

        // AddInheritedField is one logical schema operation, so trigger generation once.
        TriggerPostSaveForContentType(contentType);

        return l.ReturnTrue();
    }

    private IContentType? GetContentType(int contentTypeId)
        => AppWorkCtx.AppReader.GetContentTypeOptional(contentTypeId);

    private void TriggerPostSaveForContentType(IContentType? contentType)
    {
        if (contentType == null)
            return;

        // Runner is intentionally best-effort so editor save is never blocked by optional generation issues.
        contentTypeChangeActions.Value.RunFor(
            AppWorkCtx.AppId,
            contentType.NameId,
            source: ContentTypeChangeSources.ContentTypeField);
    }

    #endregion
}
