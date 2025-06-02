namespace ToSic.Eav.Apps.Sys.AppJson;

/// <summary>
/// Service to handle "app.json" optional json file in App_Data folder
/// with exclude configuration to define files and folders that will not be exported in app export
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppJsonConfigurationService
{
    /// <summary>
    ///  move app.json template from old to new location
    /// </summary>
    void MoveAppJsonTemplateFromOldToNewLocation();

    /// <summary>
    /// Get the settings object from the App.json file in the App_Data folder
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="useShared"></param>
    /// <returns>The AppJson object, or null</returns>
    AppJsonConfiguration GetAppJson(int appId, bool useShared = false);

    /// <summary>
    /// Generate a unique cache key for this specific app.json, so that it can be watched for changes.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="useShared"></param>
    /// <returns></returns>
    string AppJsonCacheKey(int appId, bool useShared = false);

    /// <summary>
    /// Get the exclude search patterns from the App.json file in the App_Data folder
    /// </summary>
    /// <param name="sourceFolder"></param>
    /// <param name="appId"></param>
    /// <param name="useShared"></param>
    /// <returns>List&lt;string&gt; - never null</returns>
    List<string> ExcludeSearchPatterns(string sourceFolder, int appId, bool useShared = false);
}