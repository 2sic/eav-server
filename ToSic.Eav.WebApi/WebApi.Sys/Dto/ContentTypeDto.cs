using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class ContentTypeDto: IdNameDto
{
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