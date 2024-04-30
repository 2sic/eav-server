using System.IO;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.Helpers;
using ToSic.Eav.Internal.Configuration;

namespace ToSic.Eav.Apps.Internal;

/// <summary>
/// Service to handle "app.json" optional json file in App_Data folder
/// with exclude configuration to define files and folders that will not be exported in app export
/// </summary>
/// <param name="globalConfiguration"></param>
/// <param name="site"></param>
/// <param name="appStates"></param>
/// <param name="appPaths"></param>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppJsonService(LazySvc<IGlobalConfiguration> globalConfiguration, ISite site, IAppStates appStates, IAppPathsMicroSvc appPaths, MemoryCacheService memoryCacheService, Lazy<IJsonServiceInternal> json)
    : ServiceBase($"{EavLogs.Eav}.AppJsonSvc", connect: [globalConfiguration, site, appStates, appPaths, memoryCacheService, json]), IAppJsonService
{

    /// <summary>
    ///  move app.json template from old to new location
    /// </summary>
    public void MoveAppJsonTemplateFromOldToNewLocation()
    {
        var appDataProtectedFolder = new DirectoryInfo(Path.Combine(globalConfiguration.Value.GlobalFolder, Constants.AppDataProtectedFolder));
        Directory.CreateDirectory(globalConfiguration.Value.AppDataTemplateFolder);
        var oldAppJsonTemplateFilePath = Path.Combine(appDataProtectedFolder.FullName, Constants.AppJson);
        var appJsonTemplateFilePath = Path.Combine(globalConfiguration.Value.AppDataTemplateFolder, Constants.AppJson);
        if (File.Exists(oldAppJsonTemplateFilePath) && !File.Exists(appJsonTemplateFilePath))
            File.Move(oldAppJsonTemplateFilePath, appJsonTemplateFilePath);
    }

    public string GetPathToDotAppJson(int appId)
        => GetPathToDotAppJson(GetAppFullPath(appId));

    // TODO: CONSIDER making private
    // probably better to just use the pattern (or not), but not worry about file-existance in export
    public string GetPathToDotAppJson(string sourceFolder) 
        => Path.Combine(sourceFolder, Constants.AppDataProtectedFolder, Constants.AppJson);

    private string GetAppFullPath(int appId)
        => appPaths.Init(site, appStates.ToReader(appStates.GetCacheState(appId))).PhysicalPath;

    /// <summary>
    /// Get the settings object from the App.json file in the App_Data folder
    /// </summary>
    /// <param name="appId"></param>
    /// <returns>The AppJson object, or null</returns>
    public AppJson GetDotAppJson(int appId)
    {
        var l = Log.Fn<AppJson>($"{nameof(appId)}: '{appId}'");

        var cacheKey = CacheKey(appId);
        l.A($"cache key: {cacheKey}");

        if (memoryCacheService.Get(cacheKey) is AppJson appJson)
            return l.Return(appJson, "ok, cache hit");

        var pathToDotAppJson = GetPathToDotAppJson(appId);
        l.A($"path to '{Constants.AppJson}':'{pathToDotAppJson}'");

        if (!File.Exists(pathToDotAppJson))
            return l.ReturnNull($"warning, file '{Constants.AppJson}' not found");

        appJson = GetDotAppJsonInternal(pathToDotAppJson);

        // not ideal, I think null should be cached too, otherwise we do a lot of ongoing checks, especially if it doesn't exist, which is often the case
        // Maybe create a special call on the memory service for this kind of operation...
        if (appJson == null)
            return l.ReturnNull("warning, json is empty");

        memoryCacheService.Set(new(cacheKey, appJson), keys: [CacheKeyInternal(pathToDotAppJson)]);

        return l.ReturnAsOk(appJson);
    }

    private string CacheKey(int appId) => $"{nameof(AppJsonService)}:{nameof(appId)}:{appId}";

    // todo: consolidate
    private AppJson GetDotAppJsonInternal(string pathToDotAppJson)
    {
        var l = Log.Fn<AppJson>($"{nameof(pathToDotAppJson)}:'{pathToDotAppJson}'");

        // duplicate
        var cacheKey = CacheKeyInternal(pathToDotAppJson);
        l.A($"cache key: {cacheKey}");

        // duplicate
        if (memoryCacheService.Get(cacheKey) is AppJson appJson)
            return l.Return(appJson, "ok, internal cache hit");

        // duplicate
        if (!File.Exists(pathToDotAppJson))
            return l.ReturnNull($"warning, file '{Constants.AppJson}' not found");

        appJson = null;

        try
        {
            var text = File.ReadAllText(pathToDotAppJson);
            l.A($"json read from file, size:{text.Length}");

            if (string.IsNullOrEmpty(text))
                return l.ReturnNull("warning, json is empty");

            appJson = json.Value.To<AppJson>(text);
        }
        catch (Exception e)
        {
            l.Ex(e);
        }

        // duplicate, but with file path
        memoryCacheService.Set(new(cacheKey, appJson), filePaths: [pathToDotAppJson]);

        return l.ReturnAsOk(appJson);
    }
    private string CacheKeyInternal(string pathToDotAppJson) => $"{nameof(AppJsonService)}:{nameof(pathToDotAppJson)}:{pathToDotAppJson}";


    public List<string> ExcludeSearchPatterns(string sourceFolder)
    {
        var l = Log.Fn<List<string>>($"{nameof(sourceFolder)}: '{sourceFolder}'"); 

        var cacheKey = CacheKey(sourceFolder);
        l.A($"cache key: {cacheKey}");

        if (memoryCacheService.Get(cacheKey) is List<string> excludeSearchPatterns)
            return l.Return(excludeSearchPatterns, "ok, cache hit");

        excludeSearchPatterns = ExcludeSearchPatternsInternal(sourceFolder);

        if (excludeSearchPatterns == null)
            return l.ReturnNull("warning, json is empty");

        memoryCacheService.Set(new(cacheKey, excludeSearchPatterns), keys: [CacheKeyInternal(GetPathToDotAppJson(sourceFolder))]);

        return l.ReturnAsOk(excludeSearchPatterns);
    }

    private string CacheKey(string sourceFolder) => $"{nameof(AppJsonService)}:{nameof(sourceFolder)}:{sourceFolder}";

    private List<string> ExcludeSearchPatternsInternal(string sourceFolder)
    {
        var l = Log.Fn<List<string>>($"{nameof(sourceFolder)}: '{sourceFolder}'");

        var appJson = GetDotAppJsonInternal(GetPathToDotAppJson(sourceFolder));
        if (appJson.Export?.Exclude == null)
            return l.Return([], $"warning, '{Constants.AppJson}' is empty or 'export.exclude' configuration is missing");

        try
        {
            return l.Return(appJson.Export.Exclude
                .Select(e => (e.ToString()).Trim().Backslash())
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

    // TODO: MAKE extension method in Dnn DLL
    public bool DnnCompilerAlwaysUseRoslyn(int? appId)
        => appId.HasValue && GetDotAppJson(appId.Value)?.DotNet?.Compiler?.Equals("roslyn", StringComparison.OrdinalIgnoreCase) == true;
}