namespace ToSic.Sys.Security.Permissions;

public record PermissionCheckInfo(bool Allowed, Conditions Condition);