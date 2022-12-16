using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Helpers
{
    public class DataFolder
    {
        public static string GetDataRoot(string dataFolder)
        {
            return dataFolder?.EndsWith(Constants.FolderData) ?? false
                ? dataFolder.Substring(0, dataFolder.Length - Constants.FolderData.Length).TrimLastSlash()
                : dataFolder ?? string.Empty;
        }
    }
}