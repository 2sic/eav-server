namespace ToSic.Sys.Security.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public enum Conditions
{
    Undefined,
    Owner,
    Identity,
    Group,
    EnvironmentInstance,
    EnvironmentGlobal,
}