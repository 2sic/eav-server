
using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Eav.Data.Raw.Sys;
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
                TitleField = nameof(SiteLanguageDto.Culture),
                TypeName = "ZoneLanguages",
            });
    }

    private IEnumerable<IRawEntity> GetLanguages(IZoneMapper zoneMapper, ISite site)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>($"{site.Id}");

        // ReSharper disable once PossibleInvalidOperationException
        var cultures = zoneMapper.CulturesWithState(site)
            .Select(c => new SiteLanguageDto
            {
                Code = c.Code,
                Culture = c.Culture,
                IsEnabled = c.IsEnabled,
            })
            .ToList();

        var list = cultures
            .Select(language => new RawEntity(new()
            {
                { nameof(SiteLanguageDto.Code), language.Code },
                { nameof(SiteLanguageDto.Culture), language.Culture },
                { nameof(SiteLanguageDto.IsEnabled), language.IsEnabled },
                { nameof(SiteLanguageDto.NameId), language.NameId },
            }))
            .ToList<IRawEntity>();

        return l.Return(list, $"{list.Count}");
    }
}