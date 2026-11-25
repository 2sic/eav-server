using ToSic.Eav.WebApi.Admin.Sys;

namespace ToSic.Eav.WebApi.App.Sys;

/// <summary>
/// Direct access to app-content items, simple manipulations etc.
/// Should check for security at each standard call - to see if the current user may do this
/// Then we can reduce security access level to anonymous, because each method will do the security check
/// </summary>
public interface IAppDataController: IAppAdminWebApiController
{
    /// <summary>
    /// Get all Entities of specified Type
    /// </summary>
    // UI test: App administration > Data > REST API > select a content type and run the GET-all example.
    IEnumerable<IDictionary<string, object>> GetEntities(string contentType, string? appPath = default, IDictionary<string, string>? queryParams = null);

    // UI test: App administration > Data > REST API > select a content type and run the GET-one example.
    IDictionary<string, object> GetOne(string contentType, string id, string? appPath = default, IDictionary<string, string>? queryParams = null);

    // UI test: App administration > Data > REST API > select a content type and run a POST/PUT example.
    IDictionary<string, object> CreateOrUpdate(string contentType, Dictionary<string, object?> newContentItem, int? id = null, string? appPath = null);

    // UI test: App administration > Data > REST API > select a content type and run the DELETE example.
    void Delete(string contentType, string id, string? appPath = null);
}
