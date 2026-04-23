
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[PrivateApi]
[VisualQuery(
    NiceName = "Site Languages",
    NameId = "9981db56-6d4e-4b29-914c-4a223c063eb4",
    NameIds = ["System.SiteLanguages"], // Internal name for the system, used in some entity-pickers. Can change at any time.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Confidential,
    UiHint = "Languages of the current site"
)]
public class SiteLanguages : CustomDataSource
{
    public SiteLanguages(Dependencies services, LanguagesBackend languagesBackend)
        : base(services, logName: "Sxc.SitLng", connect: [languagesBackend])
    {
        ProvideOutRaw(
            () => GetLanguages(languagesBackend),
            options: () => new()
            {
                AutoId = true,
                TitleField = nameof(SiteLanguageDto.Culture),
                TypeName = "SiteLanguage",
            });
    }

    private IEnumerable<IRawEntity> GetLanguages(LanguagesBackend languagesBackend)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var list = languagesBackend
            .GetLanguages()
            .Select(IRawEntity (language) => new RawEntity(new()
            {
                { nameof(SiteLanguageDto.Code), language.Code },
                { nameof(SiteLanguageDto.Culture), language.Culture },
                { nameof(SiteLanguageDto.IsEnabled), language.IsEnabled },
                { nameof(SiteLanguageDto.NameId), language.NameId },
            }))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}