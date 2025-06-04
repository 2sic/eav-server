using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Apps.AppReader.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AppReaderExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsShared(this IAppReader appReader)
        => appReader.GetCache().Metadata.HasType(KnownDecorators.IsSharedDecoratorId);
}