using System.Text.Json.Serialization;
using ToSic.Eav.Data;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Dto;

public class EditInfoAttributeDto: EditInfoDto
{
    public EditInfoAttributeDto(IContentType contentType, IContentTypeAttribute attribute): base(contentType)
    {
        // If ReadOnly is true, then the base call already found it to be read only, and we don't add anything
        if (ReadOnly)
        {
            DisableSort = true;
            DisableRename = true;
            DisableMetadata = true; // don't disable, as it should be available
            DisableDelete = true;
            return;
        }

        // Check if the attribute is inherited from elsewhere #SharedFieldDefinition
        var sysSettings = attribute.SysSettings;
        if (sysSettings == null) return;
        ReadOnly = sysSettings.Inherit != null || sysSettings.InheritMetadataMainGuid != null;
        if (!ReadOnly) return;

        ReadOnlyMessage = "this attribute inherits from another attribute";
        EnableInherit = true;
        DisableSort = false;
        DisableDelete = false;
        DisableRename = sysSettings.InheritNameOfPrimary;
        DisableMetadata = sysSettings.InheritMetadataOfPrimary;
        DisableEdit = DisableMetadata;
    }

    [JsonIgnore(Condition = WhenWritingDefault)] public bool DisableSort { get; }
    [JsonIgnore(Condition = WhenWritingDefault)] public bool DisableRename { get; }
    [JsonIgnore(Condition = WhenWritingDefault)] public bool DisableMetadata { get; }
    [JsonIgnore(Condition = WhenWritingDefault)] public bool DisableDelete { get; }
    [JsonIgnore(Condition = WhenWritingDefault)] public bool DisableEdit { get; }
    [JsonIgnore(Condition = WhenWritingDefault)] public bool EnableInherit { get; }

}