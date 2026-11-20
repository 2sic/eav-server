using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Prefetch;

/// <summary>
/// Add prefetch data to the loading data such as related entities, link information, etc.
/// </summary>
/// <param name="prefetchProviders"></param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EditLoadActivityAddPrefetch(IAllKeyedServices<string, IEditLoadPrefetchProvider> prefetchProviders)
    : ServiceBase("Api.Prefetch"),
        IWork<EditLoadDto, EditLoadDto>
{
    public async Task<Package<EditLoadDto>> Handle(WorkContext actionCtx, Package<EditLoadDto> package)
    {
        var l = Log.Fn<Package<EditLoadDto>>();
        try
        {
            package = package with
            {
                Data = package.Data with
                {
                    Prefetch = await TryToPrefectAdditionalData(actionCtx, actionCtx.Get<int>(EditLoadContextConstants.AppId), package.Data)
                }
            };
            return l.Return(package, "prefetched");
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.Return(package, "error prefetching");
        }
    }

    private async Task<Dictionary<string, object>> TryToPrefectAdditionalData(WorkContext actionCtx, int appId, EditLoadDto editData)
    {
        try
        {
            var l = Log.Fn<Dictionary<string, object>>();

            var result2 = await Task.WhenAll(prefetchProviders
                .Select(async provider
                    => new
                    {
                        provider.Key,
                        Result = await provider.Service.Handle(actionCtx, editData.ToPackage())
                    }));

            return result2.ToDictionary(x => x.Key, x => x.Result.Data);
        }
        catch (Exception exception)
        {
            throw;
            //return Task.FromException<Dictionary<string, object>>(exception);
        }
    }
}