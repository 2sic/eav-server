namespace ToSic.Eav.Helpers;

internal class DataFolder
{
    public static string GetDataRoot(string dataFolder)
    {
        return dataFolder?.EndsWith(Constants.FolderSystem) ?? false
            ? dataFolder.Substring(0, dataFolder.Length - Constants.FolderSystem.Length).TrimLastSlash()
            : dataFolder ?? string.Empty;
    }
}