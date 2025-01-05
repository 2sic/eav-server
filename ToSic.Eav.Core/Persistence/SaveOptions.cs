using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record SaveOptions(string PrimaryLanguage, List<DimensionDefinition> Languages)
{
    public bool PreserveUntouchedAttributes { get; init; } = false;
    public bool PreserveUnknownAttributes { get; init; }= false;

    public bool SkipExistingAttributes { get; init; } = false;

    public string PrimaryLanguage
    {
        get;
        set => field = value.ToLowerInvariant();
    } = PrimaryLanguage;

    public List<DimensionDefinition> Languages { get; set; } = Languages;
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
}