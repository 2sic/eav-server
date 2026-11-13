namespace ToSic.Sys.Security.Permissions;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PermissionDeniedException(string? message) : UnauthorizedAccessException(message ?? "Permission Denied");
