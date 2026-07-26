using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.DataSources.Sys;

[ContentType(
    Name = "Scope",
    Guid = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
    Description = "Data Scope",
    Scope = "System"
)]
public record ScopeModel : RawEntity, IHasIdentityNameId
{
    public required string NameId { get; init; }

    [ContentTypeField(IsTitle = true)]
    public required string Name { get; init; }
    
    public required int TypesTotal { get; init; }
    
    public required int TypesInherited { get; init; }
    
    public required int TypesOfApp { get; init; }

    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(NameId), NameId },
        { nameof(Name), Name },
        { nameof(TypesTotal), TypesTotal },
        { nameof(TypesInherited), TypesInherited },
        { nameof(TypesOfApp), TypesOfApp },
    };
}
