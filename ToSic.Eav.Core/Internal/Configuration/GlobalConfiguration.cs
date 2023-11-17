using System;
using System.IO;
using ToSic.Eav.Helpers;
using ToSic.Eav.Internal.Loaders;

namespace ToSic.Eav.Internal.Configuration
{
    public class GlobalConfiguration : IGlobalConfiguration
    {
        /// <inheritdoc />
        public string DataFolder
        {
            get => _dataFolderAbsolute ?? Path.Combine(GlobalFolder, Constants.AppDataProtectedFolder, Constants.FolderSystem);
            set => _dataFolderAbsolute = CorrectFolderOrErrorIfInvalid(value, nameof(DataFolder));
        }

        /// <inheritdoc />
        public string DataBetaFolder
        {
            
            get => _dataFolderBetaAbsolute ?? Path.Combine(Helpers.DataFolder.GetDataRoot(DataFolder), Constants.FolderSystemBeta);
            set => _dataFolderBetaAbsolute = CorrectFolderOrErrorIfInvalid(value, nameof(DataBetaFolder));
        }

        /// <inheritdoc />
        public string DataCustomFolder
        {
            get => _dataFolderCustomAbsolute ?? Path.Combine(Helpers.DataFolder.GetDataRoot(DataFolder), Constants.FolderSystemCustom);
            set => _dataFolderCustomAbsolute = CorrectFolderOrErrorIfInvalid(value, nameof(DataCustomFolder));
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
            get => _temporaryFolder ?? Path.Combine(GlobalFolder, Constants.TemporaryFolder);
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

        /// <inheritdoc />
        public string ConfigFolder
        {
            get => _configFolder ?? Path.Combine(DataCustomFolder, FsDataConstants.ConfigFolder);
            set => _configFolder = CorrectFolderOrErrorIfInvalid(value, nameof(ConfigFolder));
        }

        /// <inheritdoc />
        public string InstructionsFolder
        {
            get => _instructionsFolder ?? Path.Combine(GlobalFolder, Constants.InstructionsFolder);
            set => _instructionsFolder = CorrectFolderOrErrorIfInvalid(value, nameof(InstructionsFolder));
        }

        /// <inheritdoc />
        public string AppDataTemplateFolder
        {
            get => _appDataTemplateFolder ?? Path.Combine(GlobalFolder, Constants.AppDataProtectedFolder, Constants.NewAppFolder);
            set => _appDataTemplateFolder = CorrectFolderOrErrorIfInvalid(value, nameof(AppDataTemplateFolder));
        }

        private static string CorrectFolderOrErrorIfInvalid(string value, string fieldName) 
            => value?.Backslash().TrimLastSlash() ?? throw new Exception(ErrorMessage(fieldName));

        private static string ErrorMessage(string fieldName) 
            => $"ISystemFoldersConfiguration.{fieldName} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

        private static string _dataFolderAbsolute;
        private static string _dataFolderBetaAbsolute;
        private static string _dataFolderCustomAbsolute;
        private static string _temporaryFolder;
        private static string _globalFolder;
        private static string _globalSiteFolder;
        private static string _assetsVirtualUrl;
        private static string _configFolder;
        private static string _instructionsFolder;
        private static string _appDataTemplateFolder;
    }
}
