namespace ToSic.Eav.WebApi.Admin;

public interface IAppInternalsController
{
    /// <summary>
    /// Get all app internals:
    /// - all content-types in the app
    /// - app configuration
    /// - app settings
    /// - app resources
    /// - metadata
    /// ...
    /// Consolidates 9 related requests in one request.
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    /// <remarks>
    /// Needs edit-permissions, as the item-list can also be accessed from the toolbar in certain cases.
    /// Will do permission checks internally.
    /// </remarks>
    AppInternalsDto Get(int appId);
}