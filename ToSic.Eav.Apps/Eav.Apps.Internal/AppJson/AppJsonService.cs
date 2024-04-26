using System.IO;
using System.Text.Json.Nodes;
using ToSic.Eav.Apps.Integration;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.Helpers;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Apps.Internal
{
    /// <summary>
    /// Service to handle "app.json" optional json file in App_Data folder
    /// with exclude configuration to define files and folders that will not be exported in app export
    /// </summary>
    /// <param name="globalConfiguration"></param>
    /// <param name="site"></param>
    /// <param name="appStates"></param>
    /// <param name="appPaths"></param>
    [PrivateApi]
    public class AppJsonService(LazySvc<IGlobalConfiguration> globalConfiguration, ISite site, IAppStates appStates, IAppPathsMicroSvc appPaths, MemoryCacheService memoryCacheService)
        : ServiceBase($"{EavLogs.Eav}.AppJsonSvc", connect: [globalConfiguration, site, appStates, appPaths, memoryCacheService]), IAppJsonService
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

        public string GetPathToDotAppJson(string sourceFolder) 
            => Path.Combine(sourceFolder, Constants.AppDataProtectedFolder, Constants.AppJson);

        private string GetAppFullPath(int appId)
            => appPaths.Init(site, appStates.ToReader(appStates.GetCacheState(appId))).PhysicalPath;



        public string GetDotAppJson(int appId)
        {
            var l = Log.Fn<string>($"{nameof(appId)}: '{appId}'");

            var cacheKey = CacheKey(appId);
            l.A($"cache key: {cacheKey}");

            if (memoryCacheService.Get(cacheKey) is string json)
                return l.Return(json, "ok, cache hit");

            var pathToDotAppJson = GetPathToDotAppJson(appId);
            l.A($"path to '{Constants.AppJson}':'{pathToDotAppJson}'");

            if (!File.Exists(pathToDotAppJson))
                return l.ReturnNull($"warning, file '{Constants.AppJson}' not found");

            json = GetDotAppJsonInternal(pathToDotAppJson);

            if (string.IsNullOrEmpty(json))
                return l.ReturnNull("warning, json is empty");

            memoryCacheService.Set(new(cacheKey, json), keys: [CacheKeyInternal(pathToDotAppJson)]);

            return l.ReturnAsOk(json);
        }
        private string CacheKey(int appId) => $"{nameof(AppJsonService)}:{nameof(appId)}:{appId}";

        //public string GetDotAppJson(string sourceFolder)
        //{
        //    var l = Log.Fn<string>($"{nameof(sourceFolder)}: '{sourceFolder}'");

        //    var cacheKey = CacheKey(sourceFolder);
        //    l.A($"cache key: {cacheKey}");

        //    if (memoryCacheService.Get(cacheKey) is string json)
        //        return l.Return(json, "ok, cache hit");

        //    var pathToDotAppJson = GetPathToDotAppJson(sourceFolder);
        //    l.A($"path to '{Constants.AppJson}':'{pathToDotAppJson}'");

        //    if (!File.Exists(pathToDotAppJson))
        //        return l.ReturnNull($"warning, file '{Constants.AppJson}' not found");

        //    json = GetDotAppJsonInternal(pathToDotAppJson);

        //    if (string.IsNullOrEmpty(json))
        //        return l.ReturnNull("warning, json is empty");

        //    var expiration = new TimeSpan(1, 0, 0);
        //    var policy = new CacheItemPolicy { SlidingExpiration = expiration };
        //    policy.ChangeMonitors.Add(memoryCacheService.CreateCacheEntryChangeMonitor([CacheKeyInternal(pathToDotAppJson)])); // cache dependency on existing cache item;        
        //    memoryCacheService.Set(new(cacheKey, json), policy);

        //    return l.ReturnAsOk(json);
        //}
        //private string CacheKey(string sourceFolder) => $"{nameof(AppJsonService)}:{nameof(sourceFolder)}:{sourceFolder}";

        private string GetDotAppJsonInternal(string pathToDotAppJson)
        {
            var l = Log.Fn<string>($"{nameof(pathToDotAppJson)}:'{pathToDotAppJson}'");

            var cacheKey = CacheKeyInternal(pathToDotAppJson);
            l.A($"cache key: {cacheKey}");

            if (memoryCacheService.Get(cacheKey) is string json)
                return l.Return(json, "ok, cache hit");

            if (!File.Exists(pathToDotAppJson))
                return l.ReturnNull($"warning, file '{Constants.AppJson}' not found");

            json = null;

            try
            {
                json = File.ReadAllText(pathToDotAppJson);
                l.A($"json read from file, size:{json.Length}");
            }
            catch (Exception e)
            {
                l.Ex(e);
            }

            if (string.IsNullOrEmpty(json))
                return l.ReturnNull("warning, json is empty");

            memoryCacheService.Set(new(cacheKey, json), filePaths: [pathToDotAppJson]);

            return l.ReturnAsOk(json);
        }
        private string CacheKeyInternal(string pathToDotAppJson) => $"{nameof(AppJsonService)}:{nameof(pathToDotAppJson)}:{pathToDotAppJson}";


        public List<string> ExcludeSearchPatterns(string sourceFolder)
        {
            var l = Log.Fn<List<string>>($"{nameof(sourceFolder)}: '{sourceFolder}'");
            // validate source folder
            if (!Directory.Exists(sourceFolder))
                return l.Return([], $"warning, can't find source folder '{sourceFolder}'");

            // validate app.json content
            var jsonString = GetDotAppJsonInternal(sourceFolder);
            if (string.IsNullOrEmpty(jsonString))
                return l.Return([], $"warning, '{Constants.AppJson}' is empty");

            // deserialize app.json
            try
            {
                var json = JsonNode.Parse(jsonString, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions);
                return l.Return(json?["export"]?["exclude"]?.AsArray()
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
    }
}
