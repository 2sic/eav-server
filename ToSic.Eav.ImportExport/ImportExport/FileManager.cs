using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using ToSic.Eav.Helpers;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Serialization;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.IO.Path;

namespace ToSic.Eav.ImportExport
{
    public class FileManager : ServiceBase
    {
        private const char Separator = ';';

        public FileManager() : base(EavLogs.Eav + ".FileMn") { }

        //public FileManager(string sourceFolder) : base("FileMan")
        //{
        //    _sourceFolder = sourceFolder;
        //}

        public FileManager SetFolder(string sourceFolder, string subFolder = null)
        {
            _sourceFolder = sourceFolder; // we need to preserve it as "AppRoot" to get app.json
            _root =
                string.IsNullOrEmpty(
                    sourceFolder) // we need it as "root" for files for export, root is usually same or under "sourceFolder"
                    ? string.Empty
                    : string.IsNullOrEmpty(subFolder)
                        ? sourceFolder
                        : Combine(sourceFolder, subFolder);
            return this;
        }

        private string _sourceFolder;
        private string _root;

        /// <summary>
        /// Copy all files from SourceFolder to DestinationFolder (directly on the file system)
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="overwriteFiles"></param>
        /// <param name="messages"></param>
        public void CopyAllFiles(string destinationFolder, bool overwriteFiles, List<Message> messages
        ) => Log.Do($"dest:{destinationFolder}, overwrite:{overwriteFiles}", () =>
        {
            var filteredFiles = GetAllTransferableFiles().ToList();
            Log.A($"copy files:{filteredFiles.Count}");
            foreach (var file in filteredFiles)
            {
                var relativeFilePath = file.Replace(_root, "");
                var destinationFilePath = $"{destinationFolder}{DirectorySeparatorChar}{relativeFilePath}";
                Log.A($"relFilePath:{relativeFilePath},destFilePath:{destinationFilePath}");

                Directory.CreateDirectory(GetDirectoryName(destinationFilePath) ?? string.Empty);

                if (!File.Exists(destinationFilePath))
                    File.Copy(file, destinationFilePath, overwriteFiles);
                else
                {
                    var alreadyExistsMsg = "File '" + GetFileName(destinationFilePath) +
                                           "' not copied because it already exists";
                    Log.A($"warning: {alreadyExistsMsg}");
                    messages.Add(new Message(alreadyExistsMsg, Message.MessageTypes.Warning));
                }
            }
        });

        /// <summary>
        /// Get exclude search patterns from app.json
        /// </summary>
        /// <returns></returns>
        private List<string> ExcludeSearchPatterns => _excludeSearchPatterns.Get(() => Log.Func(l =>
        {
            // validate source folder
            if (!Directory.Exists(_sourceFolder))
                return (new List<string>(), $"warning, can't find source folder '{_sourceFolder}'");

            // validate app.json content
            var jsonString = File.ReadAllText(GetPathToDotAppJson(_sourceFolder));
            if (string.IsNullOrEmpty(jsonString))
                return (new List<string>(), $"warning, '{Constants.AppJson}' is empty");

            // deserialize app.json
            try
            {
                var json = JsonNode.Parse(jsonString, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions);
                return (json?["export"]?["exclude"]?.AsArray()
                    .Select(e => (e.ToString()).Trim().Backslash())
                    .Where(e => !string.IsNullOrEmpty(e) && !e.StartsWith("#")) // ignore empty lines, or comment lines that start with #
                    .Select(e => e.StartsWith(@"\") ? Combine(_sourceFolder, e.Substring(1)) : e) // handle case with starting slash
                    .Select(e => e.ToLowerInvariant())
                    .ToList(),"ok");
            }
            catch (Exception e)
            {
                l.Ex(e);
                return (new List<string>(), $"warning, json is not valid in '{Constants.AppJson}'");
            }
        }));
        private readonly GetOnce<List<string>> _excludeSearchPatterns = new GetOnce<List<string>>();


        /// <summary>
        /// Gets all files from a folder and subfolder, which fit the import/export filter criteria
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTransferableFiles(string searchPattern = "*.*") => Log.Func(l =>
        {
            var hardcodedExcludedFolders = GetHardcodedExcludedFolders();
            l.A($"Hardcoded excluded folders count: {hardcodedExcludedFolders.Count}");

            var pathToDotAppJson = GetPathToDotAppJson(_sourceFolder);
            IEnumerable<string> files;

            if (File.Exists(pathToDotAppJson))
            {
                l.A($"Exclude files based on {pathToDotAppJson}");
                files = ExcludeFilesBasedOnExcludeInDotAppJson(_root, searchPattern);
            }
            else
            {
                l.A($"Can't find {pathToDotAppJson}, exclude files using old way");
                files = AllFiles(searchPattern);
            }

            l.A($"Process excluding files based on hardcoded exclusions");
            var filteredFiles = files
                .Where(file => !FileManager.IsFileInExcludedFolder(file, hardcodedExcludedFolders))
                .ToList();

            l.A($"Returning filtered files based on exclusion criteria");
            return (filteredFiles, "Done filtering files based on exclusion criteria");
        });

        private List<string> GetHardcodedExcludedFolders() => Log.Func(l =>
        {
            // add folder slashes to ensure the term is part of a folder, not within a file-name
            var exclAnyFolder = Settings.ExcludeFolders
                .Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}")
                .ToList();

            var exclRootFolders = Settings.ExcludeRootFolders
                .Select(f => $"{Combine(_root, f)}{DirectorySeparatorChar}")
                .ToList();

            var result = exclAnyFolder.Union(exclRootFolders).ToList();
            return (result, "Ok, building list of hardcoded excluded folders");
        });

        private static bool IsFileInExcludedFolder(string filePath, IEnumerable<string> excludedFolders) 
            => excludedFolders.Any(ex => filePath.ToLowerInvariant().Contains(ex.ToLowerInvariant()));


        /// <summary>
        /// Gets all folders from a folder and subfolder, which fit the import/export filter criteria
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTransferableFolders(string searchPattern = "*.*") => Log.Func(l =>
        {
            var pathToDotAppJson = GetPathToDotAppJson(_sourceFolder);
            if (File.Exists(pathToDotAppJson)) // exclude files based on app.json from v14.08
                return (ExcludeFoldersBasedOnExcludeInDotAppJson(_root, searchPattern).ToList(), $"ok, exclude folders based on {pathToDotAppJson}");

            // old hardcoded way of excluding files
            l.A($"can't find {pathToDotAppJson}, exclude folders on old way");
            // add folder slashes to ensure the term is part of a folder, not within a file-name
            var exclAnyFolder = Settings.ExcludeFolders
                .Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}")
                .ToList();
            var exclRootFolders = Settings.ExcludeRootFolders
                .Select(f => $"{Combine(_root, f)}{DirectorySeparatorChar}").ToList();
            var excFolders = exclAnyFolder.Union(exclRootFolders).ToList();
            Log.A($"hardcoded, excFolders:{excFolders.Count()}");

            return (AllFolders(searchPattern)
                .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex.ToLowerInvariant())))
                .ToList(),$"ok, exclude folders on old way");
        });


        /// <summary>
        /// Get all files from a folder, not caring if it will be exported or not
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllFiles(string searchPattern = "*.*") =>
            Directory.Exists(_root)
                ? searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateFiles(_root, s, SearchOption.AllDirectories)).ToList()
                : new List<string>();


        /// <summary>
        /// Get all folders from a folder, not caring if it will be exported or not
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllFolders(string searchPattern = "*.*") =>
            Directory.Exists(_root)
                ? searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories)).ToList()
                : new List<string>();


        /// <summary>
        /// Exclude files based on 2sxc special exclude array in app.json
        /// </summary>
        /// <param name="root"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private IEnumerable<string> ExcludeFilesBasedOnExcludeInDotAppJson(string root, string searchPattern = "*.*") => Log.Func($"folderPath:{root}", l =>
        {
            // validate folderPath
            if (!Directory.Exists(root))
                return (new List<string>(), $"warning, can't find root folder path '{root}'");

            // validate exclude search patterns
            if (!ExcludeSearchPatterns.Any())
            {
                var allFiles = searchPattern.Split(Separator)
                    .SelectMany(s => Directory.EnumerateFiles(root, s, SearchOption.AllDirectories)).ToList();
                return (allFiles, $"warning, can't find 2sxc exclude in '{Constants.AppJson}'");
            }

            // prepare list of files to exclude using simple wildcard patterns
            l.A($"excludeSearchPatterns:{ExcludeSearchPatterns.Count}");

            // *** EXCLUDE FOLDERS & FILES
            var files = GetFilesRecursion(root, ExcludeSearchPatterns, null, searchPattern);

            return (files,"ok");
        });

        /// <summary>
        /// Exclude folders based on 2sxc special exclude array in app.json
        /// </summary>
        /// <param name="root"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private IEnumerable<string> ExcludeFoldersBasedOnExcludeInDotAppJson(string root, string searchPattern = "*.*") => Log.Func($"folderPath:{root}", l =>
        {
            // validate folderPath
            if (!Directory.Exists(root))
                return (new List<string>(), $"warning, can't find folder path '{_sourceFolder}'");

            // validate exclude search patterns
            if (!ExcludeSearchPatterns.Any())
            {
                var allFolders = searchPattern.Split(Separator)
                    .SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories)).ToList();
                return (allFolders, $"warning, can't find 2sxc exclude in '{Constants.AppJson}'");
            }

            // prepare list of files to exclude using simple wildcard patterns
            Log.A($"excludeSearchPatterns:{ExcludeSearchPatterns.Count}");

            // *** EXCLUDE FOLDERS & FILES
            var folders = GetFoldersRecursion(root, ExcludeSearchPatterns, null, searchPattern);

            return (folders,"ok");
        });

        private List<string> GetFilesRecursion(string folderPath, List<string> excludeSearchPatterns, List<string> allFiles = null, string searchPattern = "*.*")
        {
            if (allFiles == null) allFiles = ExcludeFiles(folderPath, excludeSearchPatterns, searchPattern).ToList();

            foreach (var folder in ExcludeFolders(folderPath, excludeSearchPatterns).ToList())
            {
                allFiles.AddRange(ExcludeFiles(folder, excludeSearchPatterns, searchPattern).ToList());
                GetFilesRecursion(folder, excludeSearchPatterns, allFiles);
            }

            return allFiles;
        }

        private List<string> GetFoldersRecursion(string folderPath, List<string> excludeSearchPatterns, List<string> allFolders = null, string searchPattern = "*.*")
        {
            if (allFolders == null) allFolders = new List<string>();

            foreach (var folder in ExcludeFolders(folderPath, excludeSearchPatterns).ToList())
            {
                allFolders.Add(folder);
                GetFoldersRecursion(folder, excludeSearchPatterns, allFolders);
            }

            return allFolders;
        }

        private static IEnumerable<string> ExcludeFolders(string folderPath, List<string> excludeSearchPatterns, string searchPattern = "*.*")
        {
            var folders = Directory.EnumerateDirectories(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
            foreach (var excludeSearchPattern in excludeSearchPatterns)
            {
                folders = folders.Where(f => !f.ToLowerInvariant().Contains(excludeSearchPattern)).ToList();
            }
            return folders;
        }

        private static IEnumerable<string> ExcludeFiles(string folderPath, List<string> excludeSearchPatterns, string searchPattern = "*.*")
        {
            var files = searchPattern.Split(Separator).SelectMany(s => Directory.EnumerateFiles(folderPath, s, SearchOption.TopDirectoryOnly)).ToList();
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