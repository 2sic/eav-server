using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppPathsMicroSvc: IAppPaths
{
    IAppPaths Init(ISite site, IAppState appState);
    bool InitDone { get; }
}