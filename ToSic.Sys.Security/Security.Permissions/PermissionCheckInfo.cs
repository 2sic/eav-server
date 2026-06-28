namespace ToSic.Sys.Security.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record PermissionCheckInfo(bool Allowed, Conditions Condition);