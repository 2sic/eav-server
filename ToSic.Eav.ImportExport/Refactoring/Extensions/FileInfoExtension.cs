using System.IO;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    // TODO2tk: These methods are used in the 2sxc user interface and can be moved to 2sxc maybe.
    public static class FileInfoExtension
    {
        public static void WriteStream(this FileInfo fileInfo, Stream fileContent)
        {
            fileInfo.Delete();  // Clear old content
            using (var fileWriter = fileInfo.OpenWrite())
            {
                fileContent.CopyTo(fileWriter);
                fileContent.Position = 0;
            }
        }

        public static Stream ReadStream(this FileInfo fileInfo)
        {
            var fileContent = new MemoryStream();
            using (var fileReader = fileInfo.OpenRead())
            {
                fileReader.CopyTo(fileContent);
                fileContent.Position = 0;
            }
            return fileContent;
        }
    }
}