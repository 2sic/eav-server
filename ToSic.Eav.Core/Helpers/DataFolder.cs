using System.IO;
using ToSic.Eav.Plumbing;

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
            var systemData = Path.Combine(Constants.AppDataProtectedFolder, Constants.FolderData);
            if (dataFolder?.EndsWith(systemData) == true)
                return dataFolder.Substring(0, dataFolder.Length - systemData.Length).TrimLastSlash();
            if (dataFolder?.EndsWith(".data") == true)
                return dataFolder.Substring(0, dataFolder.Length - ".data".Length).TrimLastSlash();
            return dataFolder ?? string.Empty;
        }
    }
}
