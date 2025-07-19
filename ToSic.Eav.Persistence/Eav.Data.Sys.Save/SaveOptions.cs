using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.EntityPair;

namespace ToSic.Eav.Data.Sys.Save;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record SaveOptions
{
    public required List<DimensionDefinition> Languages { get; init; }

    public required string PrimaryLanguage
    {
        get;
        set => field = value.ToLowerInvariant();
    }

    public bool PreserveUntouchedAttributes { get; init; } = false;

    public bool PreserveUnknownAttributes { get; init; } = false;

    public bool SkipExistingAttributes { get; init; } = false;

    public bool PreserveExistingLanguages { get; init; } = false;

    public bool PreserveUnknownLanguages { get; init; } = false;

    public bool DraftShouldBranch { get; init; } = true;

    /// <summary>
    /// 
    /// </summary>
    public bool DiscardAttributesNotInType { get; init; } = false;

    public override string ToString() =>
        $"save opts PUntouchedAt:{PreserveUntouchedAttributes}, " +
        $"PUnknownAt:{PreserveUnknownAttributes}, " +
        $"SkipExistingAt:{SkipExistingAttributes}" +
        $"ExistLangs:{PreserveExistingLanguages}, " +
        $"UnknownLangs:{PreserveUnknownLanguages}, " +
        $"draft-branch:{DraftShouldBranch}, Lang1:{PrimaryLanguage}, langs⋮{Languages?.Count}, " +
        $"DiscardAttrsNotInType:{DiscardAttributesNotInType}";

    /// <summary>
    /// Add a specific save options to a list of entities.
    /// Probably WIP, as it will probably not be needed once all code works with entity-pairs
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    public ICollection<IEntityPair<SaveOptions>> AddToAll(List<IEntity> entities)
    {
        var pairs = entities
            .Select(IEntityPair<SaveOptions> (e) => new EntityPair<SaveOptions>(e, this))
            .ToListOpt();
        return pairs;
    }
}