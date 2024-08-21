using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppPathsMicroSvc: IAppPaths
{
    IAppPaths Init(ISite site, IHas<IAppSpecsWithState> appState);

    IAppPaths Init(ISite site, IAppState appState);

    bool InitDone { get; }
}