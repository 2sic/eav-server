﻿using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ToSic.Eav.Logging;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.ImportExport.Zip
{
    public class Zipping: HasLog
    {

        public Zipping(ILog parentLog) : base("Zip.Abstrc", parentLog, "starting")
        {
        }

        public MemoryStream ZipDirectoryIntoStream(string zipDirectory)
        {
            var stream = new MemoryStream();
            var zipStream = new ZipOutputStream(stream);
            zipStream.SetLevel(6);
            ZipFolder(zipDirectory, zipDirectory, zipStream);
            zipStream.Finish();
            return stream;
        }

        public void ZipFolder(string rootFolder, string currentFolder, ZipOutputStream zStream)
        {

            var subFolders = Directory.GetDirectories(currentFolder);
            foreach (var folder in subFolders)
                ZipFolder(rootFolder, folder, zStream);

            var relativePath = currentFolder.Substring(rootFolder.Length) + "\\";
            
            foreach (var file in Directory.GetFiles(currentFolder))
                AddFileToZip(zStream, relativePath, file);
        }



        private void AddFileToZip(ZipOutputStream zStream, string relativePath, string file)
        {
            var buffer = new byte[4096];
            var fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            var entry = new ZipEntry(fileRelativePath);
            entry.DateTime = DateTime.Now;

            using (var fs = File.OpenRead(file))
            {
                // Setting the Size provides WinXP built-in extractor compatibility,
                // but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
                entry.Size = fs.Length;

                zStream.PutNextEntry(entry);

                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);

                } while (sourceBytes > 0);
            }
        }


        #region Zip Import Helpers

        /// <summary>
        /// Extracts a Zip (as Stream) to the given OutFolder directory.
        /// </summary>
        public void ExtractZipFile(Stream zipStream, string outFolder, bool allowCodeImport)
        {
            var wrapLog = Log.Call($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");
            var file = new ZipFile(zipStream);

            try
            {
                foreach (ZipEntry entry in file)
                {
                    if (entry.IsDirectory)
                        continue;
                    var fileName = entry.Name;

                    var entryStream = file.GetInputStream(entry);

                    var fullPath = Path.Combine(outFolder, fileName);
                    var directoryName = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        if(!Directory.Exists(directoryName))
                            Log.Add($"Create temp path:{directoryName} (len:{directoryName.Length})");
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fullPath.Length > 240)
                        Log.Warn($"file name is very long - could cause trouble:{fullPath}");

                    // enhanced security check
                    var isCode = FileNames.IsKnownCodeExtension(fileName);
                    if (isCode)
                    {
                        Log.Add($"code file detected:{fullPath}");
                        if (!allowCodeImport)
                        {
                            Log.Add("Code file import not permitted - will throw error");
                            wrapLog("error");
                            throw new Exception("Importing code files is not permitted - you need super-user permissions to do this. " +
                                                $"The process was stopped on the file '{fileName}'");
                        }
                    }

                    // Unzip File in buffered chunks
                    using (var streamWriter = File.Create(fullPath))
                    {
                        entryStream.CopyTo(streamWriter, 4096);
                    }
                }
            }
            finally
            {
                file.Close();
            }

            wrapLog("ok");
        }

        #endregion
    }
}
