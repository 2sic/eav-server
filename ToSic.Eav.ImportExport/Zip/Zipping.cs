using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.ImportExport.Zip
{
    public class Zipping: HasLog
    {

        public Zipping(Log parentLog) : base("Zip.Abstrc", parentLog, "starting")
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

            // 2018-04-06 2dm disabled, seems completely unused
            //if (relativePath.Length > 1)
            //{
            //    var dirEntry = new ZipEntry(relativePath);
            //    dirEntry.DateTime = DateTime.Now;
            //}

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
                entry.Size = fs.Length;
                // Setting the Size provides WinXP built-in extractor compatibility,
                //  but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.
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
        /// <param name="zipStream"></param>
        /// <param name="outFolder"></param>
        public void ExtractZipFile(Stream zipStream, string outFolder)
        {
            Log.Add($"extract zip to:{outFolder}");
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
                        Log.Add($"will create temp dir len:{fullPath.Length} path:{fullPath}");
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fullPath.Length > 240)
                        Log.Warn($"file name is very long - could cause trouble:{fullPath}");

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
        }

        #endregion
    }
}
