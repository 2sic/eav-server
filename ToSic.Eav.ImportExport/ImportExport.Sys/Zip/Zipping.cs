using System.IO.Compression;
using System.Text;
using ToSic.Eav.Security.Files;
using static ToSic.Eav.ImportExport.Sys.ImpExpConstants;

namespace ToSic.Eav.ImportExport.Sys.Zip;

internal class Zipping(ILog parentLog) : HelperBase(parentLog, "Zip.Abstrc")
{
    public MemoryStream ZipDirectoryIntoStream(string zipDirectory)
    {
        using var stream = new MemoryStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
        AddFolder(archive, zipDirectory, zipDirectory);
        return stream;
    }

    public void AddFolder(ZipArchive archive, string rootFolder, string currentFolder)
    {
        var subFolders = Directory.GetDirectories(currentFolder);
        foreach (var folder in subFolders)
            AddFolder(archive, rootFolder, folder);

        var relativePath = currentFolder.Substring(rootFolder.Length) + "\\";
        foreach (var file in Directory.GetFiles(currentFolder))
            AddFile(archive, file, relativePath);
    }

    public void AddFile(ZipArchive archive, string sourcePath, string zipPath)
    {
        var l = Log.Fn();
        var fileRelativePath = (zipPath.Length > 1 ? zipPath : string.Empty) + Path.GetFileName(sourcePath);
        archive.CreateEntryFromFile(sourcePath, fileRelativePath, CompressionLevel.Optimal);
        l.Done();
    }

    /// <summary>
    /// Add a list of files to the provided ZipArchive with explicit target paths.
    /// This avoids duplicated code in callers and ensures consistent compression settings.
    /// </summary>
    /// <param name="archive">Target archive</param>
    /// <param name="files">Tuple of sourcePath and zipPath inside archive</param>
    public void AddFiles(ZipArchive archive, IEnumerable<(string sourcePath, string zipPath)> files)
    {
        var l = Log.Fn($"{nameof(files)}:{files?.Count()}");
        foreach (var (sourcePath, zipPath) in files ?? Array.Empty<(string sourcePath, string zipPath)>())
        {
            var entry = archive.CreateEntry(zipPath, CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            using var fileStream = File.OpenRead(sourcePath);
            fileStream.CopyTo(entryStream);
            l.A($"add: {zipPath}");
        }
        l.Done("ok");
    }

    /// <summary>
    /// Add a text entry to the provided ZipArchive using UTF8 by default (no BOM).
    /// </summary>
    public void AddTextEntry(ZipArchive archive, string zipPath, string content, Encoding? encoding = null)
    {
        var enc = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        var entry = archive.CreateEntry(zipPath, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, enc);
        writer.Write(content);
    }

    /// <summary>
    /// Add a byte[] entry to the provided ZipArchive.
    /// </summary>
    public void AddBytesEntry(ZipArchive archive, string zipPath, byte[] bytes)
    {
        var entry = archive.CreateEntry(zipPath, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(bytes, 0, bytes.Length);
    }


    #region Zip Import Helpers

    /// <summary>
    /// Extracts a Zip (as Stream) to the given OutFolder directory.
    /// </summary>
    public void ExtractZipStream(Stream zipStream, string outFolder, bool allowCodeImport, bool ignoreFolderEntries = false)
    {

        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}, {nameof(ignoreFolderEntries)}:{ignoreFolderEntries}");

        using var zipArchive = new ZipArchive(zipStream);
        ExtractZipArchiveToFile(zipArchive, outFolder, allowCodeImport, ignoreFolderEntries);

        l.Done("ok");
    }

    /// <summary>
    /// Extracts a Zip (as File) to the given OutFolder directory.
    /// </summary>
    public void ExtractZipFile(string zipPath, string outFolder, bool allowCodeImport, bool ignoreFolderEntries = false)
    {
        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}, {nameof(ignoreFolderEntries)}:{ignoreFolderEntries}");

        using var zipArchive = ZipFile.OpenRead(zipPath);
        ExtractZipArchiveToFile(zipArchive, outFolder, allowCodeImport, ignoreFolderEntries);

        l.Done("ok");
    }

    private void ExtractZipArchiveToFile(ZipArchive zipArchive, string outFolder, bool allowCodeImport, bool ignoreFolderEntries = false)
    {
        var l = Log.Fn($"{nameof(outFolder)}:'{outFolder}', {nameof(allowCodeImport)}:{allowCodeImport}");

        // 2025-08-01 2dm: I spent more than an hour trying to import a zip which turned out not to be an app export.
        // so I'm adding a check for what caused the error and throwing.
        var realEntries = zipArchive.Entries
            .Where(e => e.Name != "")
            .ToListOpt();

        // If count is off, there are entries for empty folders - which is an indication that it's not a proper app export.
        if (realEntries.Count != zipArchive.Entries.Count && !ignoreFolderEntries)
            throw new("Zip contained entries for folders, which never happens in normal App exports. This is probably not a 2sxc app.");

        foreach (var entry in ignoreFolderEntries ? realEntries : zipArchive.Entries)
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