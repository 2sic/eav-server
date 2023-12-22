using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Services
{
    public interface IAppContentTypeService
    {
        /// <summary>
        /// All content types of the app.
        /// </summary>
        IEnumerable<IContentType> ContentTypes { get; }

        /// <summary>
        /// Get a single content type by name (display name or NameId).
        /// </summary>
        /// <param name="name">the name, either the normal name or the NameId which looks like a GUID</param>
        /// <returns></returns>
        IContentType GetContentType(string name);

        IContentType GetContentType(int contentTypeId);

    }
}
