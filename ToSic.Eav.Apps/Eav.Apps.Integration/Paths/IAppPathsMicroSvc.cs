using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Integration;

public interface IAppPathsMicroSvc: IAppPaths
{
    IAppPaths Init(ISite site, IAppState appState);
    bool InitDone { get; }
}