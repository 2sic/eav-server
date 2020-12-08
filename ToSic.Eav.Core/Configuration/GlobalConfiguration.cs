using System;
using ToSic.Eav.Helpers;

namespace ToSic.Eav.Configuration
{
    public class GlobalConfiguration : IGlobalConfiguration
    {
        private const string Placeholder = "{name}";
        private const string ErrMessage = "ISystemFoldersConfiguration.{name} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

        /// <inheritdoc />
        public string DataFolder
        {
            get => _dataFolderAbsolute;
            set => _dataFolderAbsolute = value?.Backslash().TrimLastSlash() ?? throw new Exception(ErrMessage.Replace(Placeholder, nameof(DataFolder)));
        }
        private static string _dataFolderAbsolute;
    }
}
