using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Sys.Paths;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppPathsMicroSvc
{
    IAppPaths Get(IAppReader appReader);

    IAppPaths Get(IAppReader appReader, ISite site = default);
}