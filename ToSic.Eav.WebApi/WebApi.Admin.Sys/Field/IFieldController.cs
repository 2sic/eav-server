namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IFieldController: IAppAdminWebApiController
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
    // UI test: App administration > Data > content type fields > Add Field.
    int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index);
    
    /// <summary>
    /// Delete a field / attribute
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="attributeId"></param>
    /// <returns></returns>
    // UI test: App administration > Data > content type fields > delete a field.
    bool Delete(int appId, int contentTypeId, int attributeId);

    /// <summary>
    /// Rename an field
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="attributeId"></param>
    /// <param name="newName"></param>
    // UI test: App administration > Data > content type fields > rename a field and save.
    void Rename(int appId, int contentTypeId, int attributeId, string newName);

    /// <summary>
    /// Change the order of the fields
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="contentTypeId"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    // UI test: App administration > Data > content type fields > drag a field to a new position.
    bool Sort(int appId, int contentTypeId, string order);

    /// <summary>
    /// Update the input-type information on a field
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="inputType"></param>
    /// <returns></returns>
    // UI test: App administration > Data > content type fields > change a field input type and save.
    bool InputType(int appId, int attributeId, string inputType);

    #region Shared Field Definitions

    /// <summary>
    /// Configure field sharing settings WIP #SharedFieldDefinition
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="share"></param>
    /// <param name="hide"></param>
    // UI test: Content type fields > field sharing > Share this field.
    bool Share(int appId, int attributeId, bool share, bool hide = false);

    /// <summary>
    /// Configure field inherit settings WIP #SharedFieldDefinition
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="attributeId"></param>
    /// <param name="inheritMetadataOf"></param>
    // UI test: Content type fields > field sharing > inherit configuration from another shared field.
    bool Inherit(int appId, int attributeId, Guid inheritMetadataOf);

    // UI test: Content type fields > Add Shared Fields > add a selected inherited field.
    bool AddInheritedField(int appId, int contentTypeId, string sourceType, Guid sourceField, string name);

    #endregion
}
