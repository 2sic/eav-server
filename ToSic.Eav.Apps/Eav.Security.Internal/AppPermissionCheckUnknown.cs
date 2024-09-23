using ToSic.Eav.Apps;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Security.Internal;

/// <inheritdoc />
internal sealed class AppPermissionCheckUnknown(IAppReaderFactory appReaders, PermissionCheckBase.MyServices services, WarnUseOfUnknown<AppPermissionCheckUnknown> _)
    : AppPermissionCheck(appReaders, services), IIsUnknown;