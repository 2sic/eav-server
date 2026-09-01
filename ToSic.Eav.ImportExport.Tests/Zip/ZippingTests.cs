using System.IO.Compression;
using System.Text;
using ToSic.Eav.ImportExport.Sys.Zip;

namespace ToSic.Eav.ImportExport.Tests.Zip;

public class ZippingTests
{
    [Fact]
    public void ZipDirectoryIntoStream_UsesPortableEntrySeparators()
    {
        var root = CreateTempFolder();
        var folder = Path.Combine(root, "nested");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "file.txt"), "ok");

        try
        {
            using var stream = new Zipping(null).ZipDirectoryIntoStream(root);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            Equal("nested/file.txt", Single(archive.Entries).FullName);
        }
        finally
        {
            Zipping.TryToDeleteDirectory(root, null);
        }
    }

    [Theory]
    [InlineData("../evil.txt")]
    [InlineData("folder/../../evil.txt")]
    [InlineData("/evil.txt")]
    [InlineData("//server/share/evil.txt")]
    [InlineData("C:/temp/evil.txt")]
    [InlineData(@"C:\temp\evil.txt")]
    public void ExtractZipStream_BlocksUnsafeEntryPath(string entryName)
    {
        // The extract helper is shared by app import and extension install. These inputs should fail
        // before any file is written, even if a platform would otherwise normalize them differently.
        using var zipStream = ZipWithEntry(entryName);
        var outFolder = CreateTempFolder();

        try
        {
            var ex = Throws<Exception>(() =>
                new Zipping(null).ExtractZipStream(zipStream, outFolder, allowCodeImport: true));

            Contains("Illegal Zip File Path", ex.Message);
        }
        finally
        {
            Zipping.TryToDeleteDirectory(outFolder, null);
        }
    }

    [Fact]
    public void ExtractZipStream_LongPathEntry_ExtractsFile()
    {
        // This reproduces the practical failure mode: each path segment is valid, but the final
        // Windows path crosses legacy MAX_PATH once the ZIP entry is placed under the temp folder.
        const string content = "long path content";
        var segment = new string('a', 70);
        var entryName = $"extensions/demo/dist/{segment}/{segment}/{segment}/file.txt";
        using var zipStream = ZipWithEntry(entryName, content);
        var outFolder = CreateTempFolder();
        var targetPath = Path.Combine(outFolder, entryName.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            True(Path.GetFullPath(targetPath).Length >= 260);

            new Zipping(null).ExtractZipStream(zipStream, outFolder, allowCodeImport: true);

            True(File.Exists(Zipping.PathForDiskAccess(targetPath)));
            Equal(content, File.ReadAllText(Zipping.PathForDiskAccess(targetPath)));
        }
        finally
        {
            Zipping.TryToDeleteDirectory(outFolder, null);
        }
    }

    [Fact]
    public void ExtractZipStream_WindowsSegmentTooLong_FailsBeforeExtract()
    {
        if (Path.DirectorySeparatorChar != '\\')
            return;

        // A single path component above the NTFS segment limit cannot be saved even with \\?\.
        // The code should reject this as a compatibility/preflight error instead of pretending the
        // extended-path fallback can solve it.
        var segment = new string('a', 256);
        using var zipStream = ZipWithEntry($"extensions/demo/{segment}/file.txt");
        var outFolder = CreateTempFolder();

        try
        {
            var ex = Throws<InvalidOperationException>(() =>
                new Zipping(null).ExtractZipStream(zipStream, outFolder, allowCodeImport: true));

            Contains("Windows path segment longer than 255 characters", ex.Message);
        }
        finally
        {
            Zipping.TryToDeleteDirectory(outFolder, null);
        }
    }

    private static MemoryStream ZipWithEntry(string entryName, string content = "ok")
    {
        // Build the ZIP in memory so tests can use names that would be unsafe or impossible as real
        // files on the current filesystem. That keeps the test focused on ZIP extraction behavior.
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry(entryName);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
            writer.Write(content);
        }
        stream.Position = 0;
        return stream;
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "2sxc-zip-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
