using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Eav.Models;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Settings;

internal static class LoadSettingsForBase
{

    public static Dictionary<string, object> GetSettings<TData, TModel>(
        PropertyStack appSettings,
        TData defaults,
        bool forceDefaults,
        string lookupPath,
        string outputPath,
        Func<TModel, TData> modelConverter,
        ILog log)
        where TModel : class, IModelFromEntity
    {
        var l = log.Fn<Dictionary<string, object>>();
        var data = defaults;

        if (!forceDefaults)
        {
            var getSettings = appSettings.InternalGetPath(lookupPath);
            if (getSettings?.GetFirstResultEntity() is { } mapsEntity)
            {
                var model = mapsEntity.ToModel<TModel>();
                if (model != null)
                    data = modelConverter(model);
            }
        }

        var result = new Dictionary<string, object>
        {
            [outputPath] = data!,
        };
        return l.Return(result, $"{result.Count}");
    }
}