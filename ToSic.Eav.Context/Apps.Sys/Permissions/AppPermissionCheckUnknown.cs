using ToSic.Sys.Security.Permissions;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Sys.Permissions;

internal sealed class AppPermissionCheckUnknown(IAppReaderFactory appReaders, PermissionCheckBase.MyServices services, WarnUseOfUnknown<AppPermissionCheckUnknown> _)
    : AppPermissionCheck(appReaders, services), IIsUnknown;