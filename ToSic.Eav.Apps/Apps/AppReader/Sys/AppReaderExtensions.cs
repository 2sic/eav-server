namespace ToSic.Eav.Apps.Internal.MetadataDecorators;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AppReaderExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsShared(this IAppReader appReader)
        => appReader.GetCache().Metadata.HasType(Metadata.Decorators.IsSharedDecoratorId);
}