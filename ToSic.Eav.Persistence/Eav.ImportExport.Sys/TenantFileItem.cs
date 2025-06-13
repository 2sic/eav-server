namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Helper class to manage file references from the original Id
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class TenantFileItem
{
    public required int Id;
    public required string? Path;
    public required string? RelativePath;
}