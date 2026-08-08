using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Name = "Scope",
    Guid = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
    Description = "Data Scope",
    Scope = "System"
)]
public record ScopeModel : IRawEntityAutoConvert
{
    public required string NameId { get; init; }

    [ContentTypeTitle]
    public required string Name { get; init; }
    
    public required int TypesTotal { get; init; }
    
    public required int TypesInherited { get; init; }
    
    public required int TypesOfApp { get; init; }

}
