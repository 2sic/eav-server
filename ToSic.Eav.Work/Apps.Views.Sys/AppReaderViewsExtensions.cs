using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys;
using ToSic.Sys.Caching.PiggyBack;

namespace ToSic.Eav.Apps.Views.Sys;

public static class AppReaderViewsExtensions
{
    public static List<IEntity> GetAllViewEntities(this WorkEntities appEntities)
        => appEntities.MyOptions.AppReader
        .GetCache()
        .PiggyBackGetExpiring(() => appEntities
            .Get(AppConstants.TemplateContentType)
            .ToList()
        ).Value;
}
