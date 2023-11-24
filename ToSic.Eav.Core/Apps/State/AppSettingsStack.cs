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
        Owner = owner;
        return this;
    }

    private AppState Owner { get; set; }
}