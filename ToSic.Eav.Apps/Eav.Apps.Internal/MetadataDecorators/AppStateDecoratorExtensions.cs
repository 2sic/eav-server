using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateDecoratorExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsShared(this IAppSpecsWithStateAndCache appState)
        => appState.Cache.Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
}