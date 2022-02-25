using System.Collections.Generic;

namespace ToSic.Eav.WebApi.App
{
    /// <summary>
    /// Direct access to app-content items, simple manipulations etc.
    /// Should check for security at each standard call - to see if the current user may do this
    /// Then we can reduce security access level to anonymous, because each method will do the security check
    /// </summary>
    public interface IAppContentController
    {
        /// <summary>
        /// Get all Entities of specified Type
        /// </summary>
        IEnumerable<IDictionary<string, object>> GetEntities(string contentType, string appPath = null);

        IDictionary<string, object> GetOne(string contentType, string id, string appPath = null);

        IDictionary<string, object> CreateOrUpdate(string contentType, Dictionary<string, object> newContentItem, int? id = null, string appPath = null);

        void Delete(string contentType, string id, string appPath = null);
    }
}