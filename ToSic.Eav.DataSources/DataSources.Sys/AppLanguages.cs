using ToSic.Eav.Apps;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys;

[PrivateApi]
[VisualQuery(
    NiceName = "App Languages",
    NameId = "c8676078-b904-4412-bf4e-aa83d48b63e7",
    NameIds = ["System.AppLanguages"], // Internal name for the system, used in the app-admin UI (admin only). Can change at any time.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Languages of the current app"
)]
public class AppLanguages(CustomDataSource.Dependencies services, IAppReaderFactory appReaders, AppUserLanguageCheck appLanguages)
    : CustomDataSource(services, logName: "Sxc.AppLangs", connect: [appReaders, appLanguages])
{
    /// <summary>
    /// Get the site languages
    /// </summary>
    protected override IEnumerable<IRawData> GetDefault()
        => appLanguages.LanguagesWithPermissions(appReaders.Get(AppId));
}