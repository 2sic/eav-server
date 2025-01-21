using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Security.Internal;

internal sealed class AppPermissionCheckUnknown(IAppReaderFactory appReaders, PermissionCheckBase.MyServices services, WarnUseOfUnknown<AppPermissionCheckUnknown> _)
    : AppPermissionCheck(appReaders, services), IIsUnknown;