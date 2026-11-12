namespace ToSic.Eav.Apps.Install.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPlatformAppInstaller
{
    string? GetAutoInstallPackagesUiUrl(bool forContentApp);
}