using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

namespace ToSic.Eav.Context;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IContextOfApp: IContextOfSite
{
    /// <summary>
    /// The App State which the current context has
    /// </summary>
    IAppStateInternal AppState { get; }

    /// <summary>
    /// Reset call to change what AppState is in the context.
    /// Internal API to get the context ready
    /// </summary>
    void ResetApp(IAppIdentity appIdentity);

    /// <summary>
    /// WIP v15
    /// </summary>
    PropertyStack AppSettings { get; }
    /// <summary>
    /// WIP v15
    /// </summary>
    PropertyStack AppResources { get; }
}