using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Reader;

internal class AppReadContentTypes(AppState appState): IAppReadContentTypes
{
    public IEnumerable<IContentType> ContentTypes => appState.ContentTypes;

    public IContentType GetContentType(string name) => appState.GetContentType(name);

    public IContentType GetContentType(int contentTypeId) => appState.GetContentType(contentTypeId);
}