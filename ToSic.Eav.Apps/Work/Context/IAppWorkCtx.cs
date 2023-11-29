using ToSic.Eav.Apps.Reader;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Basic context for working with data at App level.
/// ATM it just transports the AppState.
/// For advanced APIs you will need the <see cref="IAppWorkCtxPlus"/>
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppWorkCtx : IAppIdentity
{
    //AppState AppState { get; }

    IAppStateInternal AppState { get; }
}