using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Persistence.Interfaces
{
    public interface IImportExportEnvironment: IHasLog
    {

        List<Message> Messages { get; }

        /// <summary>
        /// Copy all files from SourceFolder to DestinationFolder
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destinationFolder">The portal-relative path where the files should be copied to</param>
        ///// <param name="overwriteFiles"></param>
        ///// <param name="messages"></param>
        ///// <param name="fileIdMapping">The fileIdMapping is needed to re-assign the existing "File:" parameters while importing the content</param>
        void TransferFilesToTennant(string sourceFolder, string destinationFolder);

        Version TennantVersion { get; }

        string ModuleVersion { get; }

        /// <summary>
        /// This is used for import-cases, where the scope
        /// is missing in the file
        /// </summary>
        string FallbackContentTypeScope { get; }
        string DefaultLanguage { get; }
        string TemplatesRoot(int zoneId, int appId);
        string TargetPath(string folder);
        void MapExistingFilesToImportSet(Dictionary<int, string> filesAndPaths, Dictionary<int, int> fileIdMap);
        void CreateFoldersAndMapToImportIds(Dictionary<int, string> foldersAndPath, Dictionary<int, int> folderIdCorrectionList, List<Message> importLog);

        SaveOptions SaveOptions(int zoneId);
    }
}