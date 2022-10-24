namespace ToSic.Eav.Helpers
{
    public class DataFolder
    {
        //private readonly string _dataFolder;

        //public DataFolder(string dataFolder)
        //{
        //    _dataFolder = dataFolder;
        //}

        //public string DataRoot => _dataRoot.Get(() => GetDataRoot(_dataFolder));
        //private readonly GetOnce<string> _dataRoot = new GetOnce<string>();
        
        public static string GetDataRoot(string dataFolder)
        {
            return dataFolder?.EndsWith(Constants.FolderData) ?? false
                ? dataFolder.Substring(0, dataFolder.Length - Constants.FolderData.Length).TrimLastSlash()
                : dataFolder ?? string.Empty;
        }
    }
}
