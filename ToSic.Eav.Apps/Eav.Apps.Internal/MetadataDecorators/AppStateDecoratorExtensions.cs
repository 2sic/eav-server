using ToSic.Eav.Apps.Internal.Specs;

namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateDecoratorExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsShared(this IAppReader appReader)
        => appReader.StateCache.Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
}