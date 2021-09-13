using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.ImportExport
{
    public class FileManager
    {

        public FileManager(string sourceFolder)
        {
            _sourceFolder = sourceFolder;

        }

        /// <summary>
        /// Folder-names of folders which won't be exported or imported
        /// </summary>

        private readonly string _sourceFolder;

        /// <summary>
        /// Copy all files from SourceFolder to DestinationFolder (directly on the file system)
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="overwriteFiles"></param>
        /// <param name="messages"></param>
        public void CopyAllFiles(string destinationFolder, bool overwriteFiles, List<Message> messages)
        {
            var filteredFiles = AllTransferableFiles;

            foreach (var file in filteredFiles)
            {
                var relativeFilePath = file.Replace(_sourceFolder, "");
                var destinationFilePath = $"{destinationFolder}{Path.DirectorySeparatorChar}{relativeFilePath}";
                
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));

                if (!File.Exists(destinationFilePath))
                    File.Copy(file, destinationFilePath, overwriteFiles);
                else
                    messages.Add(new Message("File '" + Path.GetFileName(destinationFilePath) + "' not copied because it already exists", Message.MessageTypes.Warning));
            }
        }

        private IEnumerable<string> _allTransferableFiles; 
        /// <summary>
        /// Gets all files from a folder and subfolder, which fit the import/export filter criteria
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllTransferableFiles
        {
            get
            {
                if (_allTransferableFiles == null)
                {
                    // add folder slashes to ensure the term is part of a folder, not within a file-name
                    var exclAnyFolder = Settings.ExcludeFolders.Select(f => "\\" + f + "\\").ToArray();
                    var exclRootFolders = Settings.ExcludeRootFolders.Select(f => _sourceFolder + f).ToArray();
                    var excFolders = exclAnyFolder.Union(exclRootFolders).ToArray();

                    _allTransferableFiles = AllFiles
                        .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex)));
                }
                return _allTransferableFiles;
            }
        }

        private IEnumerable<string> _allFiles; 

        /// <summary>
        /// Get all files from a folder, not caring if it will be exported or not
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllFiles => _allFiles ??
                                               (_allFiles = Directory.EnumerateFiles(_sourceFolder, "*.*", SearchOption.AllDirectories));
    }
}