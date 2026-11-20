namespace ToSic.Sys.Security.Permissions;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PermissionDeniedException(string message)
    : UnauthorizedAccessException($"Permission denied: {message ?? "Reason not specified"}");
