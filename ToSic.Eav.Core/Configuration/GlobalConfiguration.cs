using System;
using System.IO;
using ToSic.Eav.Helpers;

namespace ToSic.Eav.Configuration
{
    public class GlobalConfiguration : IGlobalConfiguration
    {
        /// <inheritdoc />
        public string DataFolder
        {
            get => _dataFolderAbsolute ?? Path.Combine(GlobalFolder, Constants.FolderData);
            set => _dataFolderAbsolute = CorrectFolderOrErrorIfInvalid(value, nameof(DataFolder));
        }

        /// <inheritdoc />
        public string GlobalFolder
        {
            get => _globalFolder ?? throw new Exception(ErrorMessage(nameof(GlobalFolder)));
            set => _globalFolder = CorrectFolderOrErrorIfInvalid(value, nameof(GlobalFolder));
        }

        /// <inheritdoc />
        public string TemporaryFolder
        {
            get => _temporaryFolder ?? Path.Combine(GlobalFolder, "_");
            set => _temporaryFolder = CorrectFolderOrErrorIfInvalid(value, nameof(TemporaryFolder));
        }

        /// <inheritdoc />
        public string SharedAppsFolder
        {
            get => _globalSiteFolder ?? throw new Exception(ErrorMessage(nameof(SharedAppsFolder)));
            set => _globalSiteFolder = CorrectFolderOrErrorIfInvalid(value, nameof(SharedAppsFolder));
        }

        /// <inheritdoc />
        public string AssetsVirtualUrl
        {
            get => _assetsVirtualUrl ?? throw new Exception(ErrorMessage(nameof(AssetsVirtualUrl)));
            set => _assetsVirtualUrl = CorrectFolderOrErrorIfInvalid(value, nameof(AssetsVirtualUrl));
        }

        private static string CorrectFolderOrErrorIfInvalid(string value, string fieldName) 
            => value?.Backslash().TrimLastSlash() ?? throw new Exception(ErrorMessage(fieldName));

        private static string ErrorMessage(string fieldName) 
            => $"ISystemFoldersConfiguration.{fieldName} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

        private static string _dataFolderAbsolute;
        private static string _temporaryFolder;
        private static string _globalFolder;
        private static string _globalSiteFolder;
        private static string _assetsVirtualUrl;
    }
}
