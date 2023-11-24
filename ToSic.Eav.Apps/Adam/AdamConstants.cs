namespace ToSic.Eav.Apps.Adam;

public class AdamConstants
{
    public const string AdamRootFolder = "adam";

    public const string TypeName = "AdamConfiguration";
    public const string ConfigFieldRootFolder = "AppRootFolder";

    public const string ItemFolderMask = "[AdamRoot]/[Guid22]/[FieldName]/[SubFolder]";
    public static string AdamFolderMask = $"{AdamRootFolder}/{AppConstants.AppFolderPlaceholder}/";

    public const int MaxSameFileRetries = 1000;
    public const int MaxUploadKbDefault = 25000;

}