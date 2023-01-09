using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using ToSic.Eav.Helpers;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Serialization;
using ToSic.Lib.Services;
using static System.IO.Path;

namespace ToSic.Eav.ImportExport
{
    public class FileManager : ServiceBase
    {
        public FileManager() : base(EavLogs.Eav + ".FileMn") { }

        //public FileManager(string sourceFolder) : base("FileMan")
        //{
        //    _sourceFolder = sourceFolder;
        //}

        public FileManager SetFolder(string sourceFolder)
        {
            _sourceFolder = sourceFolder;
            return this;
        }

        /// <summary>
        /// Folder-names of folders which won't be exported or imported
        /// </summary>

        private string _sourceFolder;

        /// <summary>
        /// Copy all files from SourceFolder to DestinationFolder (directly on the file system)
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="overwriteFiles"></param>
        /// <param name="messages"></param>
        public void CopyAllFiles(string destinationFolder, bool overwriteFiles, List<Message> messages)
        {
            var wrapLog = Log.Fn($"dest:{destinationFolder}, overwrite:{overwriteFiles}");
            var filteredFiles = AllTransferableFiles.ToList();
            Log.A($"copy files:{filteredFiles.Count}");
            foreach (var file in filteredFiles)
            {
                var relativeFilePath = file.Replace(_sourceFolder, "");
                var destinationFilePath = $"{destinationFolder}{DirectorySeparatorChar}{relativeFilePath}";
                Log.A($"relFilePath:{relativeFilePath},destFilePath:{destinationFilePath}");

                Directory.CreateDirectory(GetDirectoryName(destinationFilePath) ?? string.Empty);

                if (!File.Exists(destinationFilePath))
                    File.Copy(file, destinationFilePath, overwriteFiles);
                else
                {
                    var alreadyExistsMsg = "File '" + GetFileName(destinationFilePath) + "' not copied because it already exists";
                    Log.A($"warning: {alreadyExistsMsg}");
                    messages.Add(new Message(alreadyExistsMsg, Message.MessageTypes.Warning));
                }
            }
            wrapLog.Done("ok");
        }

        /// <summary>
        /// Gets all files from a folder and subfolder, which fit the import/export filter criteria
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllTransferableFiles
        {
            get
            {
                if (_allTransferableFiles != null) return _allTransferableFiles;

                var wrapLog = Log.Fn<IEnumerable<string>>();

                var pathToDotAppJson = GetPathToDotAppJson(_sourceFolder);
                if (File.Exists(pathToDotAppJson))
                {
                    // exclude files based on app.json from v14.08
                    _allTransferableFiles = ExcludeFilesBasedOnExcludeInDotAppJson(_sourceFolder).ToList();
                    Log.A($"exclude files based on {pathToDotAppJson}, files:{_allTransferableFiles.Count()}");
                }
                else
                {
                    // old hardcoded way of excluding files
                    Log.A($"can't find {pathToDotAppJson}, exclude files on old way");
                    // add folder slashes to ensure the term is part of a folder, not within a file-name
                    var exclAnyFolder = Settings.ExcludeFolders.Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}").ToList();
                    var exclRootFolders = Settings.ExcludeRootFolders.Select(f => $"{Combine(_sourceFolder, f)}{DirectorySeparatorChar}").ToList();
                    var excFolders = exclAnyFolder.Union(exclRootFolders).ToList();
                    Log.A($"hardcoded, excFolders:{excFolders.Count()}");

                    _allTransferableFiles = AllFiles
                        .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex.ToLowerInvariant())))
                        .ToList();
                    Log.A($"hardcoded, files:{_allTransferableFiles.Count()}");
                }

                return wrapLog.ReturnAsOk(_allTransferableFiles);
            }
        }
        private IEnumerable<string> _allTransferableFiles;

        /// <summary>
        /// Get all files from a folder, not caring if it will be exported or not
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllFiles => _allFiles ??
            (_allFiles = Directory.Exists(_sourceFolder) ? Directory.EnumerateFiles(_sourceFolder, "*.*", SearchOption.AllDirectories) : new List<string>());
        private IEnumerable<string> _allFiles;

        /// <summary>
        /// Exclude files based on 2sxc special exclude array in app.json
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <returns></returns>
        private IEnumerable<string> ExcludeFilesBasedOnExcludeInDotAppJson(string sourceFolder)
        {
            var wrapLog = Log.Fn<IEnumerable<string>>($"folderPath:{sourceFolder}");

            // validate source folder
            if (!Directory.Exists(_sourceFolder))
                return wrapLog.Return(new List<string>(), $"warning, can't find folder '{_sourceFolder}'");

            // validate app.json content
            var jsonString = File.ReadAllText(GetPathToDotAppJson(_sourceFolder));
            if (string.IsNullOrEmpty(jsonString))
            {
                // nothing to filter, just return all files for export
                var allFiles = Directory.EnumerateFiles(_sourceFolder, "*.*", SearchOption.AllDirectories);
                return wrapLog.Return(allFiles, $"warning, '{Constants.AppJson}' is empty");
            }

            List<string> excludeSearchPatterns;
            // validate json
            try
            {
                var json = JsonNode.Parse(jsonString, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions);
                excludeSearchPatterns = json?["export"]?["exclude"]?.AsArray()
                    .Select(e => (e.ToString()).Trim().Backslash())
                    .Where(e => !string.IsNullOrEmpty(e) && !e.StartsWith("#")) // ignore empty lines, or comment lines that start with #
                    .Select(e => e.StartsWith(@"\") ? Combine(_sourceFolder, e.Substring(1)) : e) // handle case with starting slash
                    .Select(e => e.ToLowerInvariant())
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Ex(e);
                var allFiles = Directory.EnumerateFiles(_sourceFolder, "*.*", SearchOption.AllDirectories);
                return wrapLog.Return(allFiles, $"warning, json is not valid in '{Constants.AppJson}'");
            }

            // validate exclude search patterns
            if (excludeSearchPatterns == null || !excludeSearchPatterns.Any())
            {
                var allFiles = Directory.EnumerateFiles(_sourceFolder, "*.*", SearchOption.AllDirectories);
                return wrapLog.Return(allFiles, $"warning, can't find 2sxc exclude in '{Constants.AppJson}'");
            }

            // prepare list of files to exclude using simple wildcard patterns
            Log.A($"excludeSearchPatterns:{excludeSearchPatterns.Count}");

            // *** EXCLUDE FOLDERS & FILES
            var files = GetFilesRecursion(sourceFolder, excludeSearchPatterns);

            return wrapLog.ReturnAsOk(files);
        }

        private List<string> GetFilesRecursion(string folderPath, List<string> excludeSearchPatterns, List<string> allFiles = null)
        {
            if (allFiles == null) allFiles = ExcludeFiles(folderPath, excludeSearchPatterns).ToList();

            foreach (var folder in ExcludeFolders(folderPath, excludeSearchPatterns).ToList())
            {
                allFiles.AddRange(ExcludeFiles(folder, excludeSearchPatterns).ToList());
                GetFilesRecursion(folder, excludeSearchPatterns, allFiles);
            }

            return allFiles;
        }

        private static IEnumerable<string> ExcludeFolders(string folderPath, List<string> excludeSearchPatterns)
        {
            var folders = Directory.EnumerateDirectories(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var excludeSearchPattern in excludeSearchPatterns)
            {
                folders = folders.Where(f => !f.ToLowerInvariant().Contains(excludeSearchPattern)).ToList();
            }
            return folders;
        }

        private static IEnumerable<string> ExcludeFiles(string folderPath, List<string> excludeSearchPatterns)
        {
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var excludeSearchPattern in excludeSearchPatterns)
            {
                files = files.Where(f => !f.ToLowerInvariant().Contains(excludeSearchPattern)).ToList();
            }
            return files;
        }

        //private IEnumerable<string> ExcludeFolders(string folderPath, List<string> excludeSearchPatterns)
        //{
        //    var folders = Directory.EnumerateDirectories(folderPath, "*.*", SearchOption.TopDirectoryOnly);
        //    // filter folders iteratively

        //    var excludeFolder = new List<string>();
        //    foreach (var excludeSearchPattern in excludeSearchPatterns)
        //    {
        //        try
        //        {
        //            var folderExcluded = Directory.EnumerateDirectories(folderPath, excludeSearchPattern.TrimPrefixSlash().TrimLastSlash().Backslash(), SearchOption.TopDirectoryOnly).ToList();
        //            if (folderExcluded.Any())
        //                excludeFolder.AddRange(folderExcluded.Select(ex => ex.ToLowerInvariant()).ToList());
        //        }
        //        catch (Exception e)
        //        {
        //            //Log.Ex(e);
        //        }
        //    }

        //    // exclude folders
        //    if (excludeFolder.Any())
        //        folders = folders.Where(f => !excludeFolder.Contains(f.ToLowerInvariant())).ToList();

        //    return folders;
        //}

        //private IEnumerable<string> ExcludeFiles(string folderPath, List<string> excludeSearchPatterns)
        //{
        //    var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
        //    // filter folders iteratively

        //    var excludeFiles = new List<string>();
        //    foreach (var excludeSearchPattern in excludeSearchPatterns)
        //    {
        //        try
        //        {
        //            var filesExcluded = Directory.EnumerateFiles(folderPath, excludeSearchPattern.TrimPrefixSlash().Backslash(), SearchOption.TopDirectoryOnly).ToList();
        //            if (filesExcluded.Any())
        //                excludeFiles.AddRange(filesExcluded.Select(file => file.ToLowerInvariant()).ToList());
        //        }
        //        catch (Exception e)
        //        {
        //            //Log.Ex(e);
        //        }
        //    }

        //    // exclude folders
        //    if (excludeFiles.Any())
        //        files = files.Where(f => !excludeFiles.Contains(f.ToLowerInvariant())).ToList();

        //    return files;
        //}

        private static string GetPathToDotAppJson(string sourceFolder) => Combine(sourceFolder, Constants.AppDataProtectedFolder, Constants.AppJson);

    }
}