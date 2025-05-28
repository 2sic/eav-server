namespace ToSic.Eav.ImportExport.Internal;

/// <summary>
/// Helper class to manage file references from the original Id
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class TenantFileItem
{
    public int Id;
    public string Path;
    public string RelativePath;
}