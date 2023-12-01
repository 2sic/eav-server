using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Services
{
    public interface IAppContentTypeService
    {
        IEnumerable<IContentType> ContentTypes { get; }

        IContentType GetContentType(string name);

        IContentType GetContentType(int contentTypeId);

    }
}
