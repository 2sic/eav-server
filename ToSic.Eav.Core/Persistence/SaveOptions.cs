﻿using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record SaveOptions
{
    public required List<DimensionDefinition> Languages { get; init; }

    public required string PrimaryLanguage
    {
        get => field;
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
}