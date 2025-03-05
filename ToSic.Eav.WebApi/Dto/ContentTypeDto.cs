using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Dto;

public class ContentTypeDto: IdNameDto
{
    public string Label { get; set; }
    // TODO: @2dm - remove this as soon as it's not used in the UI anymore 2024-09-26
    public string StaticName { get; set; }
    // TODO: @2dm - use this in the UI instead of StaticName 2024-09-26
    public string NameId{ get; set; }
    public string Scope { get; set; }
    public string Description { get; set; }
    public bool UsesSharedDef { get; set; }
    public int? SharedDefId { get; set; }
    public int Items { get; set; }
    public int Fields { get; set; }

    public string TitleField { get; set; }

    public IEnumerable<EavLightEntityReference> Metadata { get; set; }
    public IDictionary<string, object> Properties { get; set; }

    public HasPermissionsDto Permissions { get; set; }

    public EditInfoDto EditInfo { get; set; }

}