﻿namespace ToSic.Eav.ImportExport
{
    public class Settings
    {
        public static string FileVersion = "07.00.00";
        public static string MinimumRequiredDnnVersion = "07.04.02";

        public static string[] ExcludeFolders =
        {
            ".git",
            "node_modules",
            "bower_components",
            ".vs",
        };

        public static string[] ExcludeRootFolders =
        {
            Constants.FolderData
        };

        // Keep this commented till End of June 2022 #cleanUp #oldTemplates #2631
        public const string TemplateContentType = "2SexyContent-Template";

        /// <summary>
        /// This value was used a long time (ca. 2sxc v1-3) to assign Templates with the content-types they should show
        /// </summary>
        public const int OldMetadataIdForTemplateAddOns = 15;
    }
}