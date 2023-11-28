using ToSic.Eav.Apps.Reader;
using ToSic.Lib.Documentation;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class AppSettingsStack: ServiceBase
{
    public AppSettingsStack(IAppStates appStates): base("App.Stack")
    {
        _appStates = appStates;
    }

    private readonly IAppStates _appStates;

    /// <summary>
    /// </summary>
    /// <param name="owner"></param>
    public AppSettingsStack Init(AppState owner)
    {
        //Owner = owner;
        Reader = owner.ToInterface(Log);
        return this;
    }

    /// <summary>
    /// </summary>
    public AppSettingsStack Init(IAppState state)
    {
        Reader = (IAppStateInternal)state;
        return this;
    }

    private IAppStateInternal Reader { get; set; }
}