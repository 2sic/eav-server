using ToSic.Eav.Security;

namespace ToSic.Lib.Security.Permissions;

public record PermissionCheckInfo(bool Allowed, Conditions Condition);