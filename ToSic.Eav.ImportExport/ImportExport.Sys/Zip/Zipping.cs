using System.IO.Compression;
using System.Text;
using ToSic.Eav.Security.Files;

namespace ToSic.Eav.ImportExport.Sys.Zip;

internal class Zipping(ILog? parentLog) : HelperBase(parentLog, "Zip.Abstrc")
{
    public MemoryStream ZipDirectoryIntoStream(string zipDirectory)
    {
        zipDirectory = zipDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        // Create the memory stream and keep it open until we return it to the caller
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddFolder(archive, zipDirectory, zipDirectory);
        }
        // Reset to beginning so callers can read from start
        stream.Position = 0;
        return stream;
    }

    public void AddFolder(ZipArchive archive, string rootFolder, string currentFolder)
    {
        var subFolders = Directory.GetDirectories(currentFolder);
        foreach (var folder in subFolders)
            AddFolder(archive, rootFolder, folder);

        var relativePath = currentFolder.Substring(rootFolder.Length)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ForwardSlash();
        if (relativePath.Length > 0)
            relativePath += "/";
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
        foreach (var (sourcePath, zipPath) in files ?? [])
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

        // Normalize the extraction root once and keep the trailing slash. The slash is important:
        // without it, a malicious target like "C:\temp-app-evil" could pass a naive StartsWith
        // check against "C:\temp-app". All ZIP entries later compare against this canonical root.
        var outFolderFull = EnsureTrailingDirectorySeparator(Path.GetFullPath(outFolder));

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
            // First reject obviously unsafe entry names, then build and validate the final target
            // path against the extraction root. This two-layer check protects both cheap string
            // attacks ("../", absolute paths) and subtle Path.Combine / separator behavior.
            CheckZipEntry(entry);

            var fullPath = GetSafeExtractPath(outFolderFull, entry);

            // The \\?\ fallback only helps with the total Windows path length. It does not make a
            // single file or folder segment above the NTFS component limit valid, so fail early with
            // an actionable error instead of letting CreateDirectory / ExtractToFile throw a vague one.
            var pathLimitError = WindowsPathLimitError(fullPath, entry.FullName);
            if (pathLimitError != null)
                throw new InvalidOperationException(pathLimitError);

            var directoryName = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                // Use extended paths only for the physical filesystem calls. Logs and validation stay
                // on the normal path form so diagnostics match what admins see in DNN/Oqtane folders.
                var diskDirectoryName = PathForDiskAccess(directoryName);
                try
                {
                    if (!Directory.Exists(diskDirectoryName))
                        l.A($"Create temp path:{directoryName} (len:{directoryName.Length})");
                    Directory.CreateDirectory(diskDirectoryName);
                }
                catch (Exception ex) when (IsFileSystemException(ex))
                {
                    throw new IOException(FileSystemErrorMessage("creating target directory for ZIP entry", entry.FullName, fullPath), ex);
                }
            }

            if (fullPath.Length > 240)
                l.W($"file name is very long - will use long-path fallback when possible:{fullPath} (len:{fullPath.Length})");

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
            try
            {
                // ExtractToFile is the final disk write, so it must receive the same extended path
                // treatment as directory creation. The ZIP entry name itself remains unchanged.
                entry.ExtractToFile(PathForDiskAccess(fullPath));
            }
            catch (Exception ex) when (IsFileSystemException(ex))
            {
                throw new IOException(FileSystemErrorMessage("extracting ZIP entry", entry.FullName, fullPath), ex);
            }
        }
        l.Done("ok");
    }

    // Check for illegal zip file path
    public static void CheckZipEntry(ZipArchiveEntry input)
    {
        var fullName = input.FullName.ForwardSlash();

        // Do not rely on the later full-path containment check alone. Blocking absolute and rooted
        // entries here keeps malicious packages from depending on platform-specific path quirks.
        var isRooted =
            fullName.StartsWith("/", StringComparison.Ordinal) ||
            fullName.Length >= 2 && fullName[1] == ':';
        var hasTraversal = fullName
            .Split('/')
            .Any(part => part == "..");

        if (string.IsNullOrWhiteSpace(fullName) || isRooted || hasTraversal)
            throw new("Illegal Zip File Path");
    }

    #endregion

    internal static string? WindowsPathLimitError(string fullPath, string? zipPath)
    {
        // This deliberately checks only per-segment length. Long total paths are handled by the
        // extended-path fallback; overlong individual segments cannot be represented on NTFS.
        if (!IsWindowsFileSystem())
            return null;

        var tooLongSegment = PathSegments(fullPath)
            .FirstOrDefault(segment => segment.Length > WindowsMaxPathSegment);
        if (tooLongSegment == null)
            return null;

        return
            $"Cannot extract ZIP entry '{zipPath}' because the target path contains a Windows path segment longer than {WindowsMaxPathSegment} characters. " +
            $"Segment length:{tooLongSegment.Length}; target path:'{DisplayPath(fullPath)}' (len:{DisplayPath(fullPath).Length}).";
    }

    internal static string FileSystemErrorMessage(string action, string zipPath, string targetPath)
    {
        // Keep the original exception as InnerException, but make the public message useful for
        // support: the logs should show the ZIP entry, final target, both lengths, and the likely fix.
        var displayPath = DisplayPath(targetPath);
        var longPathHint = IsWindowsFileSystem() && displayPath.Length >= WindowsLegacyMaxPath
            ? " The path is longer than the legacy Windows MAX_PATH limit. 2sxc tried the Windows extended path fallback; if this server still rejects it, enable Windows long paths or use a shorter physical root."
            : string.Empty;

        return
            $"Error {action}. ZIP entry:'{zipPath}' (len:{zipPath.Length}), target path:'{displayPath}' (len:{displayPath.Length}).{longPathHint}";
    }

    internal static bool IsFileSystemException(Exception ex)
        => ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;

    internal static string PathForDiskAccess(string path)
    {
        // Windows legacy MAX_PATH problems are caused by filesystem APIs, not by ZIP entry names.
        // Prefixing with \\?\ moves disk I/O onto the extended path namespace while keeping the
        // archive's internal paths unchanged. Already-prefixed paths are returned untouched.
        if (!IsWindowsFileSystem() || string.IsNullOrWhiteSpace(path) || path.StartsWith(ExtendedPathPrefix, StringComparison.Ordinal))
            return path;

        // On net48, Path.GetFullPath itself can throw for an already-absolute long path before the
        // fallback is applied, so only normalize relative paths here.
        var fullPath = Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(path);
        return fullPath.StartsWith(UncPrefix, StringComparison.Ordinal)
            ? ExtendedUncPathPrefix + fullPath.Substring(UncPrefix.Length)
            : ExtendedPathPrefix + fullPath;
    }

    internal static string DisplayPath(string path)
    {
        // Convert extended Windows paths back to normal display paths for logs, lock-file comparisons,
        // and relative-path calculations. The \\?\ prefix is an implementation detail, not package data.
        if (path.StartsWith(ExtendedUncPathPrefix, StringComparison.Ordinal))
            return UncPrefix + path.Substring(ExtendedUncPathPrefix.Length);

        return path.StartsWith(ExtendedPathPrefix, StringComparison.Ordinal)
            ? path.Substring(ExtendedPathPrefix.Length)
            : path;
    }

    private static string GetSafeExtractPath(string outFolderFull, ZipArchiveEntry entry)
    {
        // Build the final extraction path from the normalized root and the ZIP entry. The entry name
        // is allowed to contain forward slashes because that is the ZIP convention; convert only for
        // filesystem access.
        var entryPath = entry.FullName
            .ForwardSlash()
            .Replace('/', Path.DirectorySeparatorChar);

        // Check segment length before Path.GetFullPath. On .NET Framework, GetFullPath can throw
        // PathTooLongException first, which would bypass the clearer compatibility message.
        var pathLimitError = WindowsPathLimitError(entryPath, entry.FullName);
        if (pathLimitError != null)
            throw new InvalidOperationException(pathLimitError);

        var fullPath = Path.Combine(outFolderFull, entryPath);

        // Normalizing is still preferred because it collapses platform path details before containment
        // checks. For long Windows paths we already blocked traversal in CheckZipEntry, so avoid
        // GetFullPath only when it would reintroduce legacy MAX_PATH failures.
        if (!IsWindowsFileSystem() || fullPath.Length < WindowsLegacyMaxPath)
            fullPath = Path.GetFullPath(fullPath);

        if (!fullPath.StartsWith(outFolderFull, PathComparison))
            throw new("Illegal Zip File Path");

        return fullPath;
    }

    private static IEnumerable<string> PathSegments(string path)
        // Drive labels ("C:") are not filesystem segments for NTFS component-length purposes.
        => DisplayPath(path)
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar)
            .Where(segment => segment.HasValue() && !segment.EndsWith(":", StringComparison.Ordinal));

    private static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
           path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static StringComparison PathComparison
        => IsWindowsFileSystem()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static bool IsWindowsFileSystem()
        => Path.DirectorySeparatorChar == '\\';

    private const int WindowsLegacyMaxPath = 260;
    private const int WindowsMaxPathSegment = 255;
    private const string ExtendedPathPrefix = @"\\?\";
    private const string ExtendedUncPathPrefix = @"\\?\UNC\";
    private const string UncPrefix = @"\\";

    /// <summary>
    /// Try to delete folder
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="log"></param>
    public static void TryToDeleteDirectory(string directoryPath, ILog? log)
    {
        var l = log.Fn($"{nameof(directoryPath)}:'{directoryPath}'");
        var retryDelete = 0;

        // Extraction temp folders can now contain long paths. Cleanup must use the same disk path
        // fallback as extraction, otherwise a successful import could leave undeletable temp files.
        var diskDirectoryPath = PathForDiskAccess(directoryPath);
        do
        {
            try
            {
                if (Directory.Exists(diskDirectoryPath))
                {
                    RemoveReadOnlyRecursive(directoryPath, log);
                    Directory.Delete(diskDirectoryPath, true);
                }
            }
            catch (Exception e)
            {
                ++retryDelete;
                l.Ex(e);
                l.A("Delete ran into issues, will ignore. " +
                    "Probably files/folders are used by another process like anti-virus. " +
                    $"Retry: {retryDelete}.");
            }
        } while (Directory.Exists(diskDirectoryPath) && retryDelete <= 20);

        l.Done(Directory.Exists(diskDirectoryPath) ? "error, can't delete" : "ok");
    }

    private static void RemoveReadOnlyRecursive(string directoryPath, ILog? log)
    {
        // Read-only cleanup walks the same long-path tree that TryToDeleteDirectory removes. Convert
        // paths back to display form while enumerating so logs stay readable, then reapply the disk
        // prefix for each filesystem mutation.
        var diskDirectoryPath = PathForDiskAccess(directoryPath);
        if (!Directory.Exists(diskDirectoryPath))
            return;

        foreach (var file in Directory.GetFiles(diskDirectoryPath, "*", SearchOption.AllDirectories).Select(DisplayPath))
        {
            var diskFile = PathForDiskAccess(file);
            var attributes = File.GetAttributes(diskFile);
            if (!attributes.HasFlag(FileAttributes.ReadOnly))
                continue;

            File.SetAttributes(diskFile, attributes & ~FileAttributes.ReadOnly);
            log?.A($"clear ro file:{file}");
        }

        foreach (var folder in Directory.GetDirectories(diskDirectoryPath, "*", SearchOption.AllDirectories).Select(DisplayPath))
        {
            var dirInfo = new DirectoryInfo(PathForDiskAccess(folder));
            if (!dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
                continue;

            dirInfo.Attributes &= ~FileAttributes.ReadOnly;
            log?.A($"clear ro dir:{folder}");
        }

        var rootDir = new DirectoryInfo(diskDirectoryPath);
        if (rootDir.Attributes.HasFlag(FileAttributes.ReadOnly))
            rootDir.Attributes &= ~FileAttributes.ReadOnly;
    }
}
