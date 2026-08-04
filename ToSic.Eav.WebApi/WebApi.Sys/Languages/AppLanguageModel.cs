using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[ContentTypeSpecs(
    Name = "AppLanguage", 
    Guid = "c8676078-b904-4412-bf4e-aa83d48b63e7", 
    Description = "Language enabled for an app", 
    Scope = "System"
    )]
public class AppLanguageModel(SiteLanguageDto language) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)]
    public string Culture => language.Culture;

    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(SiteLanguageDto.Code), language.Code },
        { nameof(SiteLanguageDto.Culture), language.Culture },
        { nameof(SiteLanguageDto.IsEnabled), language.IsEnabled },
        { nameof(SiteLanguageDto.IsAllowed), language.IsAllowed },
        { nameof(SiteLanguageDto.NameId), language.NameId },
        { nameof(SiteLanguageDto.Permissions), language.Permissions },
    };
}