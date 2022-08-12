using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Helpers;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Logging;
using static System.IO.Path;

namespace ToSic.Eav.ImportExport
{
    public class FileManager : HasLog
    {
        /// <summary>
        /// optional json file in app root folder with exclude configuration
        /// to define files and folders that will not be exported in app export
        /// </summary>
        private const string DotAppJson = ".app.json";

        public FileManager(string sourceFolder) : base("FileMan")
        {
            _sourceFolder = sourceFolder;
        }

        /// <summary>
        /// Folder-names of folders which won't be exported or imported
        /// </summary>

        private readonly string _sourceFolder;

        public FileManager Init(ILog parentLog)
        {
            Log.LinkTo(parentLog);
            return this;
        }

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

                // add folder slashes to ensure the term is part of a folder, not within a file-name
                var exclAnyFolder = Settings.ExcludeFolders.Select(f => $"{DirectorySeparatorChar}{f}{DirectorySeparatorChar}").ToList();
                var exclRootFolders = Settings.ExcludeRootFolders.Select(f => $"{Combine(_sourceFolder, f)}{DirectorySeparatorChar}").ToList();
                var excFolders = exclAnyFolder.Union(exclRootFolders).ToList();
                Log.A($"step 1, excFolders:{excFolders.Count()}");

                _allTransferableFiles = AllFiles
                    .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex.ToLowerInvariant())))
                    .ToList();
                Log.A($"step 1, files:{_allTransferableFiles.Count()}");

                _allTransferableFiles = ExcludeFilesBasedOnExcludeInDotAppJson(_sourceFolder, _allTransferableFiles.ToList());

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
        /// Exclude files based on 2sxc special exclude array in .app.json
        /// using simple wildcard patterns (like in dir cmd)
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="allTransferableFiles">array with all file paths to exclude from</param>
        /// <returns></returns>
        /// <remarks>
        /// better git-ignore like implementation can use globbing with Microsoft.Extensions.FileSystemGlobbing
        /// https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing
        /// that was skipped in initial implementation because it is additional external dependency
        /// </remarks>
        private IEnumerable<string> ExcludeFilesBasedOnExcludeInDotAppJson(string sourceFolder, List<string> allTransferableFiles)
        {
            var wrapLog = Log.Fn<IEnumerable<string>>($"sourceFolder:{sourceFolder},allTransferableFiles:{allTransferableFiles.Count}");

            var pathToDotAppJson = Combine(sourceFolder, DotAppJson);
            if (!File.Exists(pathToDotAppJson)) return wrapLog.Return(allTransferableFiles, $"ok, can't find '{DotAppJson}'");

            var jsonString = File.ReadAllText(pathToDotAppJson);
            if (string.IsNullOrEmpty(jsonString)) return wrapLog.Return(allTransferableFiles, $"warning, '{DotAppJson}' is empty");

            try
            {
                var jObject = JObject.Parse(jsonString);
                var excludeSearchPatterns = jObject["export"]?["exclude"]?.Select(e => ((string)e).Backslash().ToLowerInvariant()).ToArray();
                if (excludeSearchPatterns == null) return wrapLog.Return(allTransferableFiles, $"ok, can't find 2sxc exclude in '{DotAppJson}'");

                var excludeFiles = new List<string>();

                // prepare list of files to exclude using simple wildcard patterns
                Log.A($"excludeSearchPatterns:{excludeSearchPatterns.Count()}");
                foreach (var excludeSearchPattern in
                         excludeSearchPatterns) // excludeSearchPattern parameter can contain a combination of valid literal path and wildcard (* and ?) characters
                {
                    try
                    {
                        var files = Directory.EnumerateFiles(sourceFolder, excludeSearchPattern, SearchOption.AllDirectories);
                        excludeFiles.AddRange(files.Select(ex => ex.ToLowerInvariant()));
                    }
                    catch (Exception e)
                    {
                        Log.Ex(e);
                    }
                }

                // exclude files
                Log.A($"excludeFiles:{excludeFiles.Count}");
                if (excludeFiles.Any())
                    allTransferableFiles = allTransferableFiles.Where(f => !excludeFiles.Contains(f.ToLowerInvariant())).ToList();

                Log.A($"allTransferableFiles:{allTransferableFiles.Count}");
            }
            catch (Exception e)
            {
                Log.A("error, json is not valid");
                Log.Ex(e);
            }
            return wrapLog.ReturnAsOk(allTransferableFiles);
        }
    }
}