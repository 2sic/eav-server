using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[ContentType(
    Name = "AppLanguage",
    Guid = "c8676078-b904-4412-bf4e-aa83d48b63e7",
    Description = "Language enabled for an app",
    Scope = "System"
)]
public record AppLanguageModel(SiteLanguageDto language) : RawEntity
{
    [ContentTypeField(IsTitle = true)]
    public string Culture => language.Culture;

    protected override IDictionary<string, object?> GetValues() => new Dictionary<string, object?>
    {
        { nameof(SiteLanguageDto.Code), language.Code },
        { nameof(SiteLanguageDto.Culture), language.Culture },
        { nameof(SiteLanguageDto.IsEnabled), language.IsEnabled },
        { nameof(SiteLanguageDto.IsAllowed), language.IsAllowed },
        { nameof(SiteLanguageDto.NameId), language.NameId },
        { nameof(SiteLanguageDto.Permissions), language.Permissions },
    };
}