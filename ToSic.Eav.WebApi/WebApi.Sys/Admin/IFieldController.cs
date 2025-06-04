using ToSic.Eav.Apps.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

public interface IFieldController
{
    /// <summary>
    /// Add a field to the content type
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="staticName"></param>
    /// <param name="type">the primary type, like "string" or "entity"</param>
    /// <param name="inputType">the input-type to be used - like "string-default"</param>
    /// <param name="index">position in the field-list</param>
    /// <returns></returns>
    int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index);

    /// <summary>
    /// Request available data-types in this app
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    string[] DataTypes(int appId);


    /// <summary>
    /// Delete a field / attribute
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="attributeId"></param>
    /// <returns></returns>
    bool Delete(int appId, int contentTypeId, int attributeId);


    /// <summary>
    /// Get the fields of a content-type
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="staticName"></param>
    /// <returns></returns>
    IEnumerable<ContentTypeFieldDto> All(int appId, string staticName);


    /// <summary>
    /// Get a list of all known input types in this app.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    /// <remarks>
    /// It's important to note that each app could have its own additional input types.
    /// </remarks>
    List<InputTypeInfo> InputTypes(int appId);

    /// <summary>
    /// Reserved field names which shouldn't be used in content-type fields. Also contains information why a name is reserved.
    /// </summary>
    /// <returns></returns>
    Dictionary<string, string> ReservedNames();
        
    /// <summary>
    /// Rename an field
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="attributeId"></param>
    /// <param name="newName"></param>
    void Rename(int appId, int contentTypeId, int attributeId, string newName);

    /// <summary>
    /// Change the order of the fields
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    bool Sort(int appId, int contentTypeId, string order);

    /// <summary>
    /// Update the input-type information on a field
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="inputType"></param>
    /// <returns></returns>
    bool InputType(int appId, int attributeId, string inputType);

    #region Shared Field Definitions

    IEnumerable<ContentTypeFieldDto> GetSharedFields(int appId, int attributeId = default);

    /// <summary>
    /// Configure field sharing settings WIP #SharedFieldDefinition
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="share"></param>
    /// <param name="hide"></param>
    bool Share(int appId, int attributeId, bool share, bool hide = false);

    /// <summary>
    /// Configure field inherit settings WIP #SharedFieldDefinition
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="inheritMetadataOf"></param>
    bool Inherit(int appId, int attributeId, Guid inheritMetadataOf);

    bool AddInheritedField(int appId, int contentTypeId, string sourceType, Guid sourceField, string name);

    IEnumerable<ContentTypeFieldDto> GetAncestors(int appId, int attributeId);

    IEnumerable<ContentTypeFieldDto> GetDescendants(int appId, int attributeId);

    #endregion
}