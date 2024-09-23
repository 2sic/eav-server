using ToSic.Lib.Services;

namespace ToSic.Eav.Apps;

/// <summary>
/// Base object for things that have a full app-identity (app-id and zone-id) and can also log their state.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class AppBase<TServices>(TServices services, string logName, object[] connect)
    : ServiceBase<TServices>(services, logName ?? "App.Base", connect: connect), IAppIdentity
    where TServices : MyServicesBase
{
    /// <inheritdoc />
    public int ZoneId { get; private set; }

    /// <inheritdoc />
    public int AppId { get; private set; }

    /// <summary>
    /// App identity containing zone/app combination
    /// </summary>
    /// <param name="app">the identity</param>
    protected void InitAppBaseIds(IAppIdentity app)
    {
        ZoneId = app.ZoneId;
        AppId = app.AppId;
    }
}