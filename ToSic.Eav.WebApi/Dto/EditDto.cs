using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.Dto;

public class EditDto
{
    /// <summary>
    /// The items in the load/save package.
    /// </summary>
    public List<BundleWithHeader<JsonEntity>> Items;

    /// <summary>
    /// Content-Type information for the form
    /// </summary>
    public List<JsonContentType> ContentTypes;

    /// <summary>
    /// Relevant input-types and their default labels & configuration
    /// </summary>
    public List<InputTypeInfo> InputTypes;

    /// <summary>
    /// If this set is to be published - default true
    /// </summary>
    public bool IsPublished = true;

    /// <summary>
    /// If this set is to not-published, whether it should branch, leaving the original published
    /// </summary>
    public bool DraftShouldBranch = false;

    /// <summary>
    /// Experimental additional data for configuration
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<JsonEntity> ContentTypeItems;

    /// <summary>
    /// The UI Context information
    /// </summary>
    public ContextDto Context;

    /// <summary>
    /// WIP - should contain pre-fetched data to reduce callbacks just to look up data which obviously will be needed
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EditPrefetchDto Prefetch { get; set; }

    /// <summary>
    /// WIP v15 - should contain additional settings such as default GPS coordinates
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EditSettingsDto Settings { get; set; }
}

public class EditSettingsDto
{
    public IDictionary<string, object> Values { get; set; }
    public List<JsonEntity> Entities { get; set; }

    /// <summary>
    /// New v17 ContentTypes which are used for Pickers to determine names etc.
    /// </summary>
    public List<JsonContentType> ContentTypes { get; set; }
}