using System.Runtime.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Caching;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MemoryCacheService() : ServiceBase("Eav.MemCacheSrv")
{
    private static readonly MemoryCache Cache = MemoryCache.Default;

    #region General
    public static bool Contains(string key, string regionName = null) => Cache.Contains(key, regionName);
    public static object Get(string key, string regionName = null) => Cache.Get(key, regionName);
    public object Remove(string key, string regionName = null) => Cache.Remove(key, regionName);

    public void Set(string key, object value, CacheItemPolicy policy, string regionName = null) => Cache.Set(key, value, policy, regionName);
    public void Set(CacheItem item, CacheItemPolicy policy) => Cache.Set(item, policy);

    public bool Add(string key, object value, CacheItemPolicy policy, string regionName = null) => Cache.Add(key, value, policy, regionName);



    //public void Set(string key, object value, CacheItemPolicy policy) => Cache.Add(key, value, policy);

    //public bool Contains(string key) => Cache.Contains(key);

    //public T Get<T>(string key)
    //{
    //    try
    //    {
    //        if (Cache.Contains(key))
    //        {
    //            return (T)Cache.Get(key);
    //        }
    //    }
    //    catch
    //    {
    //        // Handle exceptions or ignore
    //    }
    //    return default(T);
    //}

    //public bool TryGetValue<T>(string key, out T value)
    //{
    //    try
    //    {
    //        if (Contains(key))
    //        {
    //            value = (T)Cache.Get(key);
    //            return true;
    //        }
    //    }
    //    catch
    //    {
    //        // Handle exceptions or ignore
    //    }
    //    value = default(T);
    //    return false;
    //}

    //public T GetOrBuild<T>(string key, Func<T> buildFunc, CacheItemPolicy policy)
    //{
    //    if (TryGetValue<T>(key, out var value)) return value;
    //    value = buildFunc();
    //    Cache.Add(key, value, policy);
    //    return value;
    //}

    //public void Remove(string key) => Cache.Remove(key);

    #endregion

    #region CacheKeyHelpers

    //private static string EditUiMiddlewareHtmlCacheKey(string path) => $"2sxc-edit-ui-page-{path}";

    //public const string AssemblyCacheKeyPrefix = "2sxc.AssemblyCache.Module.";

    //public static string OutputCacheManagerCacheKey(int moduleId, int pageId, int? userId, string view, string suffix, string currentCulture)
    //{
    //    var id = $"2sxc.Lightspeed.Module.p:{pageId}-m:{moduleId}";
    //    if (userId.HasValue) id += $"-u:{userId.Value}";
    //    if (view != null) id += $"-v:{view}";
    //    if (suffix != null) id += $"-s:{suffix}";
    //    if (currentCulture != null) id += $"-c:{currentCulture}";
    //    return id;
    //}

    //public static string DataSourceCatalogAppCacheKey(int appId) => $"DataSourceCatalog:AppDataSource:{appId}";

    #endregion

    #region EditUiMiddleware

    //public string GetOrBuildEditUiMiddlewareHtml(string path, Func<string> buildFunc) 
    //    => GetOrBuild(EditUiMiddlewareHtmlCacheKey(path), buildFunc, CacheItemPolicyWithFileChangeMonitor(path));

    //private static CacheItemPolicy CacheItemPolicyWithFileChangeMonitor(string filePath)
    //{
    //    var cacheItemPolicy = new CacheItemPolicy();
    //    cacheItemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor([filePath]));
    //    return cacheItemPolicy;
    //}

    #endregion

    #region AssemblyCacheManager

    //public string Add(string cacheKey, object data, IList<string> folderPaths = null, IList<string> filePaths = null)
    //    => Add(cacheKey, data, ListToDictionary(folderPaths), filePaths);

    //public string Add(string cacheKey, object data, IDictionary<string, bool> folderPaths = null, IList<string> filePaths = null)
    //{
    //    IList<ChangeMonitor> changeMonitors = [];
    //    if (!DictionaryIsEmpty(folderPaths))
    //        changeMonitors.Add(new FolderChangeMonitor(folderPaths));
    //    if (!ListIsEmpty(filePaths))
    //        changeMonitors.Add(new HostFileChangeMonitor(filePaths!));
    //    return AddInternalWithChangeMonitors(cacheKey, data, changeMonitors);
    //}

    //private IDictionary<string, bool> ListToDictionary(IList<string> folderPaths) => folderPaths.ToDictionary(p => p, p => true);
    //private static bool ListIsEmpty(IList<string> list = null) => list?.Any() != true;
    //private static bool DictionaryIsEmpty(IDictionary<string, bool> dictionary = null) => dictionary?.Any() != true;

    //private string AddInternalWithChangeMonitors(string cacheKey, object data, IList<ChangeMonitor> changeMonitor = null)
    //{
    //    var l = Log.Fn<string>($"{nameof(cacheKey)}: {cacheKey};", timer: true);

    //    var expiration = new TimeSpan(0, 0, 3600);
    //    var policy = new CacheItemPolicy { SlidingExpiration = expiration };

    //    // Try set app change folder monitor
    //    if (changeMonitor?.Any() == true)
    //        try
    //        {
    //            l.Do(message: $"add {nameof(changeMonitor)}", timer: true, action: () =>
    //            {
    //                foreach (var changeMon in changeMonitor)
    //                    policy.ChangeMonitors.Add(changeMon);
    //            });
    //        }
    //        catch (Exception ex)
    //        {
    //            l.E("Error during set app folder ChangeMonitor");
    //            l.Ex(ex);
    //            /* ignore for now */
    //            return l.ReturnAsError("error");
    //        }

    //    // Try to add to cache
    //    try
    //    {
    //        l.Do(message: $"cache set cacheKey:{cacheKey}", timer: true,
    //            action: () => Set(cacheKey, data, policy));

    //        return l.ReturnAsOk(cacheKey);
    //    }
    //    catch (Exception ex)
    //    {
    //        l.Ex(ex);
    //        /* ignore for now */
    //        return l.ReturnAsError("error");
    //    }
    //}

    #endregion

    #region OutputCacheManager

    //public string Add(string cacheKey, object data, int duration, IEavFeaturesService features,
    //    List<IAppStateChanges> appStates, IList<string> appPaths = null, CacheEntryUpdateCallback updateCallback = null)
    //{
    //    var l = Log.Fn<string>($"key: {cacheKey}", timer: true);
    //    try
    //    {
    //        // Never store 0, that's like never-expire
    //        if (duration == 0) duration = 1;
    //        var expiration = new TimeSpan(0, 0, duration);
    //        var policy = new CacheItemPolicy { SlidingExpiration = expiration };

    //        // flush cache when any feature is changed
    //        Log.Do(message: "changeMonitors add FeaturesResetMonitor", timer: true, action: () =>
    //            policy.ChangeMonitors.Add(new FeaturesResetMonitor(features)));

    //        // get new instance of ChangeMonitor and insert it to the cache item
    //        if (appStates.Any())
    //            foreach (var appState in appStates)
    //                Log.Do(message: "changeMonitors add AppResetMonitor", timer: true, action: () =>
    //                    policy.ChangeMonitors.Add(new AppResetMonitor(appState)));

    //        if (appPaths is { Count: > 0 })
    //            Log.Do(message: "changeMonitors add FolderChangeMonitor", timer: true, action: () =>
    //                policy.ChangeMonitors.Add(new FolderChangeMonitor(appPaths)));

    //        if (updateCallback != null)
    //            policy.UpdateCallback = updateCallback;

    //        Log.Do(message: $"cache set cacheKey:{cacheKey}", timer: true, action: () => Set(cacheKey, data, policy));

    //        return l.ReturnAsOk(cacheKey);
    //    }
    //    catch
    //    {
    //        /* ignore for now */
    //    }
    //    return l.ReturnAsError("error");
    //}

    #endregion
}