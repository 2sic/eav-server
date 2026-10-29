namespace ToSic.Eav.Environment.Sys.Permissions;

/// <summary>
/// Constant keys for security, historic from Dnn
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class EnvironmentPermissionConstants
{
    public const string SalPrefix = "SecurityAccessLevel";
    public const string SalView = $"{SalPrefix}.View";
    public const string SalAnonymous = $"{SalPrefix}.Anonymous";
    public const string SalEdit = $"{SalPrefix}.Edit";
    public const string SalSiteAdmin = $"{SalPrefix}.Admin";
    public const string SalSystemUser = $"{SalPrefix}.Host";
}
