using System.Collections.Immutable;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Reader;

internal class AppReadEntities(AppState appState): IAppReadEntities
{
    public IImmutableList<IEntity> List => appState.List;

    public IEntity GetDraft(IEntity entity) => appState.GetDraft(entity);

    public IEntity GetPublished(IEntity entity) => appState.GetPublished(entity);
}