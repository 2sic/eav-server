using System;
using ToSic.Eav.Apps.Reader;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Context object for performing App modifications.
/// This should help us change all the Read/Parts etc. to be fully functional and not depend on a Parent object. 
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkCtx : IAppWorkCtx
{
    /// <inheritdoc />
    public int ZoneId { get; }

    /// <inheritdoc />
    public int AppId { get; }


    //public AppWorkCtx(AppState appState)
    //{
    //    AppId = appState.AppId;
    //    ZoneId = appState.ZoneId;
    //    AppState = appState;
    //    AppStateReader = appState.ToInterface(null).Internal();
    //}
    public AppWorkCtx(IAppState appState)
    {
        AppId = appState.AppId;
        ZoneId = appState.ZoneId;
        AppState = appState.Internal();
        //AppState = AppStateReader.StateCache;
    }

    //public AppWorkCtx(IAppWorkCtx original, AppState appState = default)
    //{
    //    if (original == null) throw new ArgumentException(@"Original must exist", nameof(original));
    //    AppId = appState?.AppId ?? original.AppId;
    //    ZoneId = appState?.ZoneId ?? original.ZoneId;
    //    AppState = appState ?? original.AppState;
    //    AppStateReader = AppState.ToInterface(null);
    //}
    public AppWorkCtx(IAppWorkCtx original, IAppStateInternal appState = default)
    {
        if (original == null) throw new ArgumentException(@"Original must exist", nameof(original));
        AppId = appState?.AppId ?? original.AppId;
        ZoneId = appState?.ZoneId ?? original.ZoneId;
        var AppState = appState?.StateCache ?? original.AppState?.StateCache;
        this.AppState = AppState.ToInterface(null);
    }


    //public AppState AppState { get; }
    public IAppStateInternal AppState { get; }

}

public static class AppWorkExtensions
{
    public static IAppWorkCtx CreateAppWorkCtx(this IAppState appState) => new AppWorkCtx(appState);
}