using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[PrivateApi]
[VisualQuery(
    NiceName = "App Languages",
    NameId = "c8676078-b904-4412-bf4e-aa83d48b63e7",
    NameIds = ["System.AppLanguages"], // Internal name for the system, used in some entity-pickers. Can change at any time.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Languages of the current app"
)]
public class AppLanguages : CustomDataSource
{
    public AppLanguages(Dependencies services, LanguagesBackend languagesBackend, LazySvc<IAppReaderFactory> appReadersLazy)
        : base(services, logName: "Sxc.AppLangs", connect: [languagesBackend, appReadersLazy])
    {
        ProvideOutRaw(
            () => GetLanguages(languagesBackend, appReadersLazy),
            options: () => new()
            {
                AutoId = true,
                TitleField = nameof(SiteLanguageDto.Culture),
                TypeName = "AppLanguages",
            });
    }

    private IEnumerable<IRawEntity> GetLanguages(LanguagesBackend languagesBackend, LazySvc<IAppReaderFactory> appReadersLazy)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var appReader = appReadersLazy.Value.Get(AppId);

        var list = languagesBackend
            .GetLanguagesOfApp(appReader, true)
            .Select(IRawEntity (language) => new RawEntity(new()
            {
                { nameof(SiteLanguageDto.Code), language.Code },
                { nameof(SiteLanguageDto.Culture), language.Culture },
                { nameof(SiteLanguageDto.IsEnabled), language.IsEnabled },
                { nameof(SiteLanguageDto.IsAllowed), language.IsAllowed },
                { nameof(SiteLanguageDto.NameId), language.NameId },
                { nameof(SiteLanguageDto.Permissions), language.Permissions },
            }))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}
