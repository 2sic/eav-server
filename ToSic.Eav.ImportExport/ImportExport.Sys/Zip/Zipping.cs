using System.IO.Compression;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.ImportExport.Sys.Zip;

internal class Zipping(ILog parentLog) : HelperBase(parentLog, "Zip.Abstrc")
{
    public MemoryStream ZipDirectoryIntoStream(string zipDirectory)
    {
        using var stream = new MemoryStream();
        using var zipStream = new ZipArchive(stream, ZipArchiveMode.Create, true);
        ZipFolder(zipDirectory, zipDirectory, zipStream);
        return stream;
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
    public void ExtractZipStream(Stream zipStream, string outFolder, bool allowCodeImport)
    {

        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");

        using var zipArchive = new ZipArchive(zipStream);
        ExtractZipArchiveToFile(zipArchive, outFolder, allowCodeImport);

        l.Done("ok");
    }

    /// <summary>
    /// Extracts a Zip (as File) to the given OutFolder directory.
    /// </summary>
    public void ExtractZipFile(string zipPath, string outFolder, bool allowCodeImport)
    {
        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");

        using var zipArchive = ZipFile.OpenRead(zipPath);
        ExtractZipArchiveToFile(zipArchive, outFolder, allowCodeImport);

        l.Done("ok");
    }

    private void ExtractZipArchiveToFile(ZipArchive zipArchive, string outFolder, bool allowCodeImport)
    {
        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");

        // 2025-08-01 2dm: I spent more than an hour trying to import a zip which turned out not to be an app export.
        // so I'm adding a check for what caused the error and throwing.
        var realEntries = zipArchive.Entries
            .Where(e => e.Name != "")
            .ToListOpt();

        // If count is off, there are entries for empty folders - which is an indication that it's not a proper app export.
        if (realEntries.Count != zipArchive.Entries.Count)
            throw new("Zip contained entries for folders, which never happens in normal App exports. This is probably not a 2sxc app.");

        foreach (var entry in zipArchive.Entries)
        {
            // check for illegal file paths in zip
            CheckZipEntry(entry);

            var fullPath = Path.Combine(outFolder, entry.FullName);
            var directoryName = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                if (!Directory.Exists(directoryName))
                    l.A($"Create temp path:{directoryName} (len:{directoryName.Length})");
                Directory.CreateDirectory(directoryName);
            }

            if (fullPath.Length > 240)
                l.W($"file name is very long - could cause trouble:{fullPath}");

            // enhanced security check
            var isCode = FileNames.IsKnownCodeExtension(entry.Name);
            if (isCode)
            {
                l.A($"code file detected:{fullPath}");
                if (!allowCodeImport)
                {
                    l.A("Code file import not permitted - will throw error");
                    l.Done("error - will throw exception");
                    throw new(
                        "Importing code files is not permitted - you need super-user permissions to do this. " +
                        $"The process was stopped on the file '{entry.FullName}'");
                }
            }

            // Unzip File
            entry.ExtractToFile(fullPath);
        }
        l.Done("ok");
    }

    // Check for illegal zip file path
    public static void CheckZipEntry(ZipArchiveEntry input)
    {
        var fullName = input.FullName.ForwardSlash();
        if (fullName.StartsWith("..") || fullName.Contains("/../"))
            throw new("Illegal Zip File Path");
    }

    #endregion

    /// <summary>
    /// Try to delete folder
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="log"></param>
    public static void TryToDeleteDirectory(string directoryPath, ILog log)
    {
        var l = log.Fn($"{nameof(directoryPath)}:'{directoryPath}'");
        var retryDelete = 0;
        do
        {
            try
            {
                if (Directory.Exists(directoryPath))
                    Directory.Delete(directoryPath, true);
            }
            catch (Exception e)
            {
                ++retryDelete;
                l.Ex(e);
                l.A("Delete ran into issues, will ignore. " +
                    "Probably files/folders are used by another process like anti-virus. " +
                    $"Retry: {retryDelete}.");
            }
        } while (Directory.Exists(directoryPath) && retryDelete <= 20);

        l.Done(Directory.Exists(directoryPath) ? "error, can't delete" : "ok");
    }
}