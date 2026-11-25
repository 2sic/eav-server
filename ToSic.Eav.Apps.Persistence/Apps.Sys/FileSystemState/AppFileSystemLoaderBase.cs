namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record AppFileSystemLoaderOptions(IAppReader AppReader, ToSic.Sys.Logging.LogSettings LogSettings);