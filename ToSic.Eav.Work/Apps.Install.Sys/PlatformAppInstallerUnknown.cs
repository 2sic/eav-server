#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Install.Sys;

internal class PlatformAppInstallerUnknown(WarnUseOfUnknown<PlatformAppInstallerUnknown> _)
    : ServiceBase($"{LogScopes.NotImplemented}.Instll"), IIsUnknown, IPlatformAppInstaller
{
    public string GetAutoInstallPackagesUiUrl(bool forContentApp) => "mvc not implemented #todo #mvc";
}