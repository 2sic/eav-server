
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Languages;

[PrivateApi]
[VisualQuery(
    NiceName = "Zone Languages",
    NameId = "9981db56-6d4e-4b29-914c-4a223c063eb4",
    NameIds = ["System.ZoneLanguages"], // Internal name for the system, used in the Admin-UI.
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Confidential,
    UiHint = "Languages of the current site"
)]
public class ZoneLanguages : CustomDataSource
{
    public ZoneLanguages(Dependencies services, LazySvc<IZoneMapper> zoneMapper, ISite site)
        : base(services, logName: "Sxc.ZoneLangs", connect: [zoneMapper, site])
    {
        ProvideOutRaw(
            () => GetLanguages(zoneMapper.Value, site),
            options: () => new()
            {
                AutoId = true,
            });
    }

    private IEnumerable<AppLanguageRaw> GetLanguages(IZoneMapper zoneMapper, ISite site)
    {
        var l = Log.Fn<IEnumerable<AppLanguageRaw>>($"{site.Id}");

        var list = zoneMapper.CulturesWithState(site)
            .Select(c => new AppLanguageRaw
            {
                Code = c.Code,
                Culture = c.Culture,
                IsEnabled = c.IsEnabled,
                IsAllowed = null,
                Permissions = null,
            })
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}
