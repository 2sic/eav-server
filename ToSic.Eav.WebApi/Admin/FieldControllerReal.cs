using ToSic.Eav.Internal.Features;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Admin;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class FieldControllerReal(
    LazySvc<ContentTypeDtoService> ctApiLazy,
    GenWorkPlus<WorkInputTypes> inputTypes,
    GenWorkDb<WorkAttributesMod> attributesMod,
    LazySvc<ISysFeaturesService> featuresSvc)
    : ServiceBase("Api.FieldRl", connect: [inputTypes, attributesMod, ctApiLazy, featuresSvc]), IFieldController
{
    public const string LogSuffix = "Field";


    #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

    public IEnumerable<ContentTypeFieldDto> All(int appId, string staticName)
        => ctApiLazy.Value.GetFields(appId, staticName);

    public string[] DataTypes(int appId)
        => attributesMod.New(appId).DataTypes();

    public List<InputTypeInfo> InputTypes(int appId)
        => inputTypes.New(appId).GetInputTypes();

    public Dictionary<string, string> ReservedNames()
        => Attributes.ReservedNames;

    public int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index)
        => attributesMod.New(appId).AddField(contentTypeId, staticName, type, inputType, index);

    public bool Delete(int appId, int contentTypeId, int attributeId)
        => attributesMod.New(appId).Delete(contentTypeId, attributeId);

    public bool Sort(int appId, int contentTypeId, string order)
        => attributesMod.New(appId).Reorder(contentTypeId, order.Trim('[', ']'));

    public bool InputType(int appId, int attributeId, string inputType) 
        => attributesMod.New(appId).SetInputType(attributeId, inputType);

    #endregion

    public void Rename(int appId, int contentTypeId, int attributeId, string newName)
        => attributesMod.New(appId).Rename(contentTypeId, attributeId, newName);

    #region Shared Fields

    public IEnumerable<ContentTypeFieldDto> GetSharedFields(int appId, int attributeId = default) 
        => ctApiLazy.Value.GetSharedFields(appId, attributeId);

    public IEnumerable<ContentTypeFieldDto> GetAncestors(int appId, int attributeId)
        => featuresSvc.Value.IsEnabled(BuiltInFeatures.ContentTypeFieldsReuseDefinitions)
            ? ctApiLazy.Value.GetAncestors(appId, attributeId)
            : [];

    public IEnumerable<ContentTypeFieldDto> GetDescendants(int appId, int attributeId)
        => featuresSvc.Value.IsEnabled(BuiltInFeatures.ContentTypeFieldsReuseDefinitions)
            ? ctApiLazy.Value.GetDescendants(appId, attributeId)
            : [];

    public bool Share(int appId, int attributeId, bool share, bool hide = false)
        => attributesMod.New(appId).FieldShare(attributeId, share, hide);

    public bool Inherit(int appId, int attributeId, Guid inheritMetadataOf)
        => attributesMod.New(appId).FieldInherit(attributeId, inheritMetadataOf);

    public bool AddInheritedField(int appId, int contentTypeId, string sourceType, Guid sourceField, string name)
        => attributesMod.New(appId).AddInheritedField(contentTypeId, sourceType, sourceField, name);

    #endregion
}