using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using static ToSic.Eav.Internal.Features.BuiltInFeatures;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkAttributesMod(
    GenWorkDb<WorkMetadata> workMetadata,
    GenWorkBasic<WorkAttributes> workAttributes,
    ContentTypeAttributeBuilder attributeBuilder,
    Generator<IDataDeserializer> dataDeserializer,
    LazySvc<IEavFeaturesService> features)
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.AttMod",
        connect: [attributeBuilder, workMetadata, workAttributes, features, dataDeserializer])
{
    #region Getters which don't modify, but need the DB

    /// <summary>
    /// Get all known data types, eg "String", "Number" etc. from DB.
    /// It should actually not be in the ...Mod because it doesn't modify anything, but it's here because it needs the DB.
    /// </summary>
    /// <returns></returns>
    public string[] DataTypes()
    {
        var l = Log.Fn<string[]>();
        var result = AppWorkCtx.DataController.Attributes.DataTypeNames();
        return l.Return(result, $"{result?.Length ?? 0}");
    }

    #endregion

    #region Add Field

    public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder)
    {
        var l = Log.Fn<int>($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
        var attDef = attributeBuilder
            .Create(appId: AppWorkCtx.AppId, name: staticName, type: ValueTypeHelpers.Get(type), isTitle: false, id: 0, sortOrder: sortOrder);
        var id = AddField(contentTypeId, attDef, inputType);
        return l.Return(id);
    }


    /// <summary>
    /// Append a new Attribute to an AttributeSet
    /// Simple overload returning int so it can be used from outside
    /// </summary>
    private int AddField(int attributeSetId, ContentTypeAttribute attDef, string inputType)
    {
        var l = Log.Fn<int>($"type:{attributeSetId}, input:{inputType}");
        var newAttribute = AppWorkCtx.DataController.Attributes.AddAttributeAndSave(attributeSetId, attDef);

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
            { AttributeMetadata.GeneralFieldInputType, inputType }
        };
        var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
        workMetadata.New(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
        l.Done();
    }

    #endregion

    #region Changes to input type, name, etc.

    public bool SetInputType(int attributeId, string inputType)
    {
        var l = Log.Fn<bool>($"attrib:{attributeId}, input:{inputType}");
        var newValues = new Dictionary<string, object> { { AttributeMetadata.GeneralFieldInputType, inputType } };

        var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
        workMetadata.New(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
        return l.ReturnTrue();
    }

    public bool Rename(int contentTypeId, int attributeId, string newName)
    {
        var l = Log.Fn<bool>($"rename attribute type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
        AppWorkCtx.DataController.Attributes.RenameAttribute(attributeId, contentTypeId, newName);
        return l.ReturnTrue();
    }

    public bool Reorder(int contentTypeId, string orderCsv)
    {
        var l = Log.Fn<bool>($"reorder type#{contentTypeId}, order:{orderCsv}");
        var sortOrderList = orderCsv.Split(',').Select(int.Parse).ToList();
        AppWorkCtx.DataController.ContentType.SortAttributes(contentTypeId, sortOrderList);
        return l.ReturnTrue();
    }


    public bool Delete(int contentTypeId, int attributeId)
    {
        var l = Log.Fn<bool>($"delete field type#{contentTypeId}, attrib:{attributeId}");
        return l.Return(AppWorkCtx.DataController.Attributes.RemoveAttributeAndAllValuesAndSave(attributeId));
    }


    #endregion

    #region New Sharing Features

    public bool FieldShare(int attributeId, bool share, bool hide = false)
    {
        var l = Log.Fn<bool>($"attributeId:{attributeId}, share:{share}, hide:{hide}");

        if (!features.Value.IsEnabled(ContentTypeFieldsReuseDefinitions.Guid))
            l.W("Setting up field share but feature is not enabled / licensed.");

        // get field attributeId
        var attribute = AppWorkCtx.DataController.Attributes.Get(attributeId)
            /*?? throw new ArgumentException($"Attribute with id {attributeId} does not exist.")*/;

        // update with the Share = share (hide we'll ignore for now, it's for future needs)
        var newSysSettings = new ContentTypeAttributeSysSettings(share: share);

        var serializer = dataDeserializer.New();
        serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

        // Update DB, and then flush the app-cache as necessary, same as any other attribute change
        AppWorkCtx.DataController.DoAndSave(() =>
        {
            // ensure GUID: update the field definition in the DB to ensure it has a GUID (but don't change if it already has one)
            if (attribute.Guid.HasValue == false) attribute.Guid = Guid.NewGuid();

            attribute.SysSettings = serializer.Serialize(newSysSettings);
        });

        return l.ReturnTrue();
    }

    public bool FieldInherit(int attributeId, Guid inheritMetadataOf)
    {
        var l = Log.Fn<bool>($"attributeId:{attributeId}, inheritMetadataOf:{inheritMetadataOf}");

        if (!features.Value.IsEnabled(ContentTypeFieldsReuseDefinitions.Guid))
            l.W("Setting up field share but feature is not enabled / licensed.");

        // get field attributeId
        var attribute = AppWorkCtx.DataController.Attributes.Get(attributeId);

        // set InheritMetadataOf to the guid above(as string)
        var newSysSettings = new ContentTypeAttributeSysSettings(
            inherit: null,
            inheritName: false,
            inheritMetadata: false,
            inheritMetadataOf: new() { [inheritMetadataOf] = "" });

        var serializer = dataDeserializer.New();
        serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

        // Update DB, and then flush the app-cache as necessary, same as any other attribute change
        AppWorkCtx.DataController.DoAndSave(() => attribute.SysSettings = serializer.Serialize(newSysSettings));

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
            .Where(f => f.Type.NameId == sourceType && f.Attribute.Guid == sourceField).ToList();

        // 1.2 Find the source fields and only keep the ones that are valid
        if (fields.Count == 0)
            return l.ReturnFalse($"error: wrong sourceType {sourceType} or sourceField {sourceField}");
        if (fields.Count > 1)
            return l.ReturnFalse($"error: we have multiple: {fields.Count} duplicate shared fields with same sourceType {sourceType} and sourceField {sourceField}");

        var pairTypeWithAttribute = fields.Single();

        // 2. Create attributes

        // 2.1 find the index for adding fields
        // - get the content-type
        var contentType = AppWorkCtx.AppReader.GetContentType(contentTypeId);
        if (contentType == null) return l.ReturnFalse($"error: wrong contentTypeId {contentTypeId}");
        // - make sure we have the attribute-count to add more fields

        // 2.2 create the attributes based on the original data
        // - name is the key in the dictionary
        // - probably just call AddField code above
        // - of course increment the start-index for each field
        var newAttributeId = AddField(contentTypeId, name,
            type: pairTypeWithAttribute.Attribute.Type.ToString(),
            inputType: pairTypeWithAttribute.Attribute.InputType(),
            sortOrder: contentType.Attributes.Count() + 1);

        // 3. Configure inherit
        FieldInherit(newAttributeId, inheritMetadataOf: sourceField);

        return l.ReturnTrue();
    }

    #endregion
}