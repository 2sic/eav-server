using System.Text.Json;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Serialization;
using ToSic.Eav.Sys.Configuration;
using ToSic.Lib.Caching;
using ToSic.Sys.Configuration;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.AppJson;

/// <inheritdoc cref="IAppJsonConfigurationService"/>
/// <param name="globalConfiguration"></param>
/// <param name="appReaders"></param>
/// <param name="appPathsFactory"></param>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppJsonConfigurationService(
    LazySvc<IGlobalConfiguration> globalConfiguration,
    IAppReaderFactory appReaders,
    IAppPathsMicroSvc appPathsFactory,
    MemoryCacheService memoryCacheService
)
    : ServiceBase($"{EavLogs.Eav}.AppJsonSvc", connect: [globalConfiguration, appReaders, appPathsFactory, memoryCacheService]),
        IAppJsonConfigurationService
{

    /// <inheritdoc />
    public void MoveAppJsonTemplateFromOldToNewLocation()
    {
        var appDataProtectedFolder = new DirectoryInfo(Path.Combine(globalConfiguration.Value.GlobalFolder(), Constants.AppDataProtectedFolder));
        var appDataTemplateFolder = globalConfiguration.Value.AppDataTemplateFolder();
        Directory.CreateDirectory(appDataTemplateFolder);
        var oldAppJsonTemplateFilePath = Path.Combine(appDataProtectedFolder.FullName, Constants.AppJson);
        var appJsonTemplateFilePath = Path.Combine(appDataTemplateFolder, Constants.AppJson);
        if (File.Exists(oldAppJsonTemplateFilePath) && !File.Exists(appJsonTemplateFilePath))
            File.Move(oldAppJsonTemplateFilePath, appJsonTemplateFilePath);
    }

    /// <inheritdoc />
    public AppJsonConfiguration GetAppJson(int appId, bool useShared = false)
    {
        var l = Log.Fn<AppJsonConfiguration>($"{nameof(appId)}: '{appId}'");

        var cacheKey = AppJsonCacheKey(appId, useShared);
        l.A($"cache key: {cacheKey}");

        if (memoryCacheService.TryGet<AppJsonConfiguration>(cacheKey, out var appJson))
            return l.Return(appJson, "ok, cache hit");

        var pathToAppJson = GetPathToAppJson(appId, useShared);
        l.A($"path to '{Constants.AppJson}':'{pathToAppJson}'");

        appJson = GetAppJsonInternal(pathToAppJson);
        if (appJson != null)
            memoryCacheService.Set(cacheKey, appJson, p => p.WatchFiles([pathToAppJson])); // cache appJson
        else
            memoryCacheService.Set(cacheKey, new AppJsonConfiguration(), p => p.WatchFolders(GetExistingParent(pathToAppJson).ToDictionary(p => p, _ => true))); // cache null

        return l.ReturnAsOk(appJson);
    }

    /// <inheritdoc />
    public string AppJsonCacheKey(int appId, bool useShared)
        => $"Eav-{nameof(AppJsonConfigurationService)}:{nameof(appId)}:{appId}:{nameof(useShared)}:{useShared}";

    /// <summary>
    /// Find parent path that exist to use it as cache dependency (folder cache monitor) 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>list with single item</returns>
    private List<string> GetExistingParent(string filePath)
    {
        var l = Log.Fn<List<string>>($"{nameof(filePath)}:'{filePath}'");

        var parentPath = Path.GetDirectoryName(filePath);

        // find parent path folder that exist
        do
        {
            if (Directory.Exists(parentPath))
                return l.ReturnAsOk([parentPath]);
            parentPath = Path.GetDirectoryName(parentPath);
        } while (!string.IsNullOrEmpty(parentPath));
        return l.Return([], "warning, parent path not found");
    }

    private string GetPathToAppJson(int appId, bool useShared)
        => Path.Combine(GetAppFullPath(appId, useShared), Constants.AppDataProtectedFolder, Constants.AppJson);

    private string GetAppFullPath(int appId, bool useShared)
    {
        var appPaths = _appPathsCache.GetOrCreate(appId, () => appPathsFactory.Get(appReaders.Get(appId)));
        return useShared ? appPaths.PhysicalPathShared : appPaths.PhysicalPath;
    }
    private readonly Dictionary<int, IAppPaths> _appPathsCache = [];

    private AppJsonConfiguration GetAppJsonInternal(string pathToAppJson)
    {
        var l = Log.Fn<AppJsonConfiguration>($"{nameof(pathToAppJson)}:'{pathToAppJson}'");

        if (!File.Exists(pathToAppJson))
            return l.ReturnNull($"file '{Constants.AppJson}' not found");

        try
        {
            var json = File.ReadAllText(pathToAppJson);
            l.A($"json read from file, size:{json.Length}");

            if (string.IsNullOrEmpty(json))
                return l.Return(new(),"json is empty");

            var appJson = JsonSerializer.Deserialize<AppJsonConfiguration>(json, JsonOptions.SafeJsonForHtmlAttributes);
            return appJson == null
                ? l.Return(new(),"appJson is null")
                : l.ReturnAsOk(appJson);
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.Return(new(), "json is not valid");
        }

    }

    /// <inheritdoc />
    public List<string> ExcludeSearchPatterns(string sourceFolder, int appId, bool useShared = false)
    {
        var l = Log.Fn<List<string>>($"{nameof(sourceFolder)}:'{sourceFolder}', {nameof(appId)}:{appId}, {nameof(useShared)}:{useShared}");

        var appJson = GetAppJson(appId, useShared);
        if (appJson?.Export?.Exclude == null)
            return l.Return([], $"warning, '{Constants.AppJson}' is missing.");  // return result; never return null, as we have a lot of .Any() checks which fail otherwise

        try
        {
            return l.Return(appJson.Export.Exclude
                .Select(e => e.ToString().Trim().Backslash())
                .Where(e => !string.IsNullOrEmpty(e) && !e.StartsWith("#")) // ignore empty lines, or comment lines that start with #
                .Select(e => e.StartsWith(@"\") ? Path.Combine(sourceFolder, e.Substring(1)) : e) // handle case with starting slash
                .Select(e => e.ToLowerInvariant())
                .ToList(), "ok");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.Return([], $"warning, json is not valid in '{Constants.AppJson}'");
        }
    }
}