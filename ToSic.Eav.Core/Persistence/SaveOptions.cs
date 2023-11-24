using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence;

public class SaveOptions
{

    public SaveOptions(string primaryLanguage, List<DimensionDefinition> languages)
    {
        PrimaryLanguage = primaryLanguage;
        Languages = languages;
    }

    public bool PreserveUntouchedAttributes = false;
    public bool PreserveUnknownAttributes = false;

    public bool SkipExistingAttributes = false;

    public string PrimaryLanguage
    {
        get => _priLang;
        set => _priLang = value.ToLowerInvariant();
    }

    private string _priLang;
    public List<DimensionDefinition> Languages = null;
    public bool PreserveExistingLanguages = false;
    public bool PreserveUnknownLanguages = false;

    public bool DraftShouldBranch = true;

    /// <summary>
    /// 
    /// </summary>
    public bool DiscardAttributesNotInType = false;

    public string LogInfo => $"save opts PUntouchedAt:{PreserveUntouchedAttributes}, " +
                             $"PUnknownAt:{PreserveUnknownAttributes}, " +
                             $"SkipExistingAt:{SkipExistingAttributes}" +
                             $"ExistLangs:{PreserveExistingLanguages}, " +
                             $"UnknownLangs:{PreserveUnknownLanguages}, " +
                             $"draft-branch:{DraftShouldBranch}, Lang1:{_priLang}, langs⋮{Languages?.Count}, " +
                             $"DiscardAttrsNotInType:{DiscardAttributesNotInType}";
}