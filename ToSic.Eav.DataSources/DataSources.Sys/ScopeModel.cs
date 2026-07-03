using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.DataSources.Sys;

[ContentTypeSpecs(
    Name = "Scope",
    Guid = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
    Description = "Data Scope",
    Scope = "System"
)]
public class ScopeModel : RawEntity, IHasIdentityNameId
{
    public required string NameId { get; init; }

    [ContentTypeAttributeSpecs(IsTitle = true)]
    public required string Name { get; init; }
    
    public required int TypesTotal { get; init; }
    
    public required int TypesInherited { get; init; }
    
    public required int TypesOfApp { get; init; }

    public override IDictionary<string, object?> Attributes(RawConvertOptions options) =>
        new Dictionary<string, object?>
        {
            { nameof(NameId), NameId },
            { nameof(Name), Name },
            { nameof(TypesTotal), TypesTotal },
            { nameof(TypesInherited), TypesInherited },
            { nameof(TypesOfApp), TypesOfApp },
        };
}
