using System;
using System.IO;
using System.IO.Compression;
using ToSic.Eav.Helpers;
using ToSic.Eav.Logging;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.ImportExport.Zip
{
    public class Zipping : HasLog
    {

        public Zipping(ILog parentLog) : base("Zip.Abstrc", parentLog, "starting")
        {
        }

        public MemoryStream ZipDirectoryIntoStream(string zipDirectory)
        {
            using (var stream = new MemoryStream())
            {
                using (var zipStream = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    ZipFolder(zipDirectory, zipDirectory, zipStream);
                }
                return stream;
            }
        }

        public void ZipFolder(string rootFolder, string currentFolder, ZipArchive zStream)
        {

            var subFolders = Directory.GetDirectories(currentFolder);
            foreach (var folder in subFolders)
                ZipFolder(rootFolder, folder, zStream);

            var relativePath = currentFolder.Substring(rootFolder.Length) + "\\";
            foreach (var file in Directory.GetFiles(currentFolder))
                AddFileToZip(zStream, relativePath, file);
        }

        private void AddFileToZip(ZipArchive zStream, string relativePath, string file)
        {
            var fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            zStream.CreateEntryFromFile(file, fileRelativePath, CompressionLevel.Optimal);
        }


        #region Zip Import Helpers

        /// <summary>
        /// Extracts a Zip (as Stream) to the given OutFolder directory.
        /// </summary>
        public void ExtractZipFile(Stream zipStream, string outFolder, bool allowCodeImport)
        {
            var wrapLog = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");
            using (var file = new ZipArchive(zipStream))
            {
                foreach (var entry in file.Entries)
                {
                    // check for illegal file paths in zip
                    CheckZipEntry(entry);

                    var fullPath = Path.Combine(outFolder, entry.FullName);
                    var directoryName = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        if (!Directory.Exists(directoryName))
                            Log.A($"Create temp path:{directoryName} (len:{directoryName.Length})");
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fullPath.Length > 240)
                        Log.W($"file name is very long - could cause trouble:{fullPath}");

                    // enhanced security check
                    var isCode = FileNames.IsKnownCodeExtension(entry.Name);
                    if (isCode)
                    {
                        Log.A($"code file detected:{fullPath}");
                        if (!allowCodeImport)
                        {
                            Log.A("Code file import not permitted - will throw error");
                            wrapLog.Done("error");
                            throw new Exception("Importing code files is not permitted - you need super-user permissions to do this. " +
                                                $"The process was stopped on the file '{entry.FullName}'");
                        }
                    }

                    // Unzip File
                    entry.ExtractToFile(fullPath);
                }
            }

            wrapLog.Done("ok");
        }

        // Check for illegal zip file path
        public static void CheckZipEntry(ZipArchiveEntry input)
        {
            var fullName = input.FullName.ForwardSlash();
            if (fullName.StartsWith("..") || fullName.Contains("/../"))
                throw new Exception("Illegal Zip File Path");
        }

        #endregion
    }
}
