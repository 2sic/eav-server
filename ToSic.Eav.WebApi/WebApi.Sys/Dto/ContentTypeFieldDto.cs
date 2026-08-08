using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "ContentTypeField",
    Guid = "eb891b1c-8505-465f-bc51-461cb47ed9c1",
    Description = "Content type field details",
    Scope = "System"
)]
public class ContentTypeFieldDto: IRawEntityAutoConvert
{
    public int Id { get; init; }
    public int SortOrder { get; init; }
    public required string Type { get; init; }
    public required string InputType { get; init; }
    
    [ContentTypeTitle]  // todo
    public required string StaticName { get; init; }
    
    public bool IsTitle { get; init; }
    public int AttributeId { get; init; }
    public required IDictionary<string, EavLightEntity> Metadata { get; init; }
    public required InputTypeInfo? InputTypeConfig { get; init; }

    public required HasPermissionsDto Permissions { get; init; }

    [JsonPropertyName("imageConfiguration")]
    public required ContentTypeFieldMetadataDto ImageConfiguration { get; init; }
        
    /// <summary>
    /// Tells the system that it will not save the field value / temporary
    /// </summary>
    /// <remarks>
    /// New in v12.01
    /// </remarks>
    public bool IsEphemeral { get; init; }
        
    /// <summary>
    /// Information if the field has calculations attached
    /// </summary>
    /// <remarks>
    /// New in v12.01
    /// </remarks>
    public bool HasFormulas { get; init; }

    public required EditInfoAttributeDto EditInfo { get; init; }

    // #SharedFieldDefinition
    public required Guid? Guid { get; init; }

    public required JsonAttributeSysSettings? SysSettings { get; init; }

    /// <summary>
    /// Short info for the case where we get the fields of many types to show
    /// </summary>
    public required JsonType? ContentType { get; init; }

    /// <summary>
    /// WIP 16.08 - list the configuration types for a field.
    /// This is so the UI knows what metadata types to request when editing the field.
    /// </summary>
    public required IDictionary<string, bool> ConfigTypes {get; init; }
}