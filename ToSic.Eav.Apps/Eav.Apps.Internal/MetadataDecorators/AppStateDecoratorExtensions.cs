using ToSic.Eav.Apps.Services;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

public static class AppStateDecoratorExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsShared(this IAppDataAndMetadataService appState)
        => appState.Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
}