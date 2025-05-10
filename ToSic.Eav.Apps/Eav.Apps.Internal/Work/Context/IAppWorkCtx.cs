using ToSic.Eav.Apps.State;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Basic context for working with data at App level.
/// ATM it just transports the AppState.
/// For advanced APIs you will need the <see cref="IAppWorkCtxPlus"/>
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtx : IAppIdentity
{
    IAppReader AppReader { get; }
}