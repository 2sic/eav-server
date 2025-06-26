namespace ToSic.Sys.Security.Permissions;

public enum Conditions
{
    Undefined,
    Owner,
    Identity,
    Group,
    EnvironmentInstance,
    EnvironmentGlobal,
}