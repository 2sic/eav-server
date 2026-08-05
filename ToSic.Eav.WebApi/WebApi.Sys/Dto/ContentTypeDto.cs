using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

[ContentType(
    Name = "ContentTypeSpecs", // "ContentType", 2026-08-05 2dm, renaming this, as it's not a real content type
    Guid = "9434ef61-5d89-4abf-9d47-d4093a37ed6f",
    Description = "Content type details",
    Scope = "System"
)]
public class ContentTypeDto: IRawEntityAutoConvert
{

    public required int Id { get; init; }
    [ContentTypeField(IsTitle = true)] // TODO: REMEMBER TO KEEP
    public required string Name { get; init; }

    public required string Label { get; init; }
    // TODO: @2dm - remove this as soon as it's not used in the UI anymore 2024-09-26
    public required string StaticName { get; init; }
    // TODO: @2dm - use this in the UI instead of StaticName 2024-09-26
    public required string NameId{ get; init; }
    public required string Scope { get; init; }
    public required string? Description { get; init; }
    public required bool UsesSharedDef { get; init; }
    public required int? SharedDefId { get; init; }
    public required int Items { get; init; }
    public required int Fields { get; init; }

    public required string? TitleField { get; init; }

    public required IEnumerable<EavLightEntityReference>? Metadata { get; init; }
    public required IDictionary<string, object>? Properties { get; init; }

    public required HasPermissionsDto Permissions { get; init; }

    public required EditInfoDto EditInfo { get; init; }

}