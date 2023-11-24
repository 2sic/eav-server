namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Basic context for working with data at App level.
/// ATM it just transports the AppState.
/// For advanced APIs you will need the <see cref="IAppWorkCtxPlus"/>
/// </summary>
public interface IAppWorkCtx : IAppIdentity
{
    AppState AppState { get; }
}