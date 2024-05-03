using System.IO;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Apps.Internal.Specs;
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

    /// <summary>
    /// Get the settings object from the App.json file in the App_Data folder
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="useShared"></param>
    /// <returns>The AppJson object, or null</returns>
    public AppJson GetAppJson(int appId, bool useShared = false)
    {
        var l = Log.Fn<AppJson>($"{nameof(appId)}: '{appId}'");

        var cacheKey = AppJsonCacheKey(appId, useShared);
        l.A($"cache key: {cacheKey}");

        if (memoryCacheService.Get(cacheKey) is AppJson appJson)
            return l.Return(appJson, "ok, cache hit");

        var pathToAppJson = GetPathToAppJson(appId, useShared);
        l.A($"path to '{Constants.AppJson}':'{pathToAppJson}'");

        appJson = GetAppJsonInternal(pathToAppJson);
        if (appJson != null)
            memoryCacheService.Set(new(cacheKey, appJson), filePaths: [pathToAppJson]); // cache appJson
        else
            memoryCacheService.Set(new(cacheKey, new AppJson()), folderPaths: GetExistingParent(pathToAppJson)); // cache null

        return l.ReturnAsOk(appJson);
    }

    public string AppJsonCacheKey(int appId, bool useShared) => $"{nameof(AppJsonService)}:{nameof(appId)}:{appId}:{nameof(useShared)}:{useShared}";

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
        if (!appPaths.InitDone) appPaths.Init(site, appStates.ToReader(appStates.GetCacheState(appId)));
        return useShared ? appPaths.PhysicalPathShared : appPaths.PhysicalPath;
    }

    private AppJson GetAppJsonInternal(string pathToAppJson)
    {
        var l = Log.Fn<AppJson>($"{nameof(pathToAppJson)}:'{pathToAppJson}'");

        if (!File.Exists(pathToAppJson))
            return l.ReturnNull($"file '{Constants.AppJson}' not found");

        AppJson appJson;
        try
        {
            var text = File.ReadAllText(pathToAppJson);
            l.A($"json read from file, size:{text.Length}");

            if (string.IsNullOrEmpty(text))
                return l.Return(new(),"json is empty");

            appJson = json.Value.To<AppJson>(text);
            if (appJson == null)
                return l.Return(new(),"appJson is null");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.Return(new(), "json is not valid");
        }

        return l.ReturnAsOk(appJson);
    }


    /// <summary>
    /// Get the exclude search patterns from the App.json file in the App_Data folder
    /// </summary>
    /// <param name="sourceFolder"></param>
    /// <param name="appId"></param>
    /// <param name="useShared"></param>
    /// <returns>List&lt;string&gt;</returns>
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
                .ToList() ?? [], "ok");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.Return([], $"warning, json is not valid in '{Constants.AppJson}'");
        }
    }
}