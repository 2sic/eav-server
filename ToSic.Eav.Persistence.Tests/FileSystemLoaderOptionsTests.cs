using ToSic.Eav.Data.Sys;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.Persistence.Tests;

public class FileSystemLoaderOptionsTests
{
    [Theory]
    [InlineData("content/system/")]
    [InlineData(@"content\system\")]
    public void Path_NormalizesSeparatorsAndKeepsTrailingSeparator(string path)
    {
        var options = new FileSystemLoaderOptions
        {
            AppId = 1,
            Path = path,
            RepoType = RepositoryTypes.Folder,
        };

        var expected = $"content{Path.DirectorySeparatorChar}system{Path.DirectorySeparatorChar}";
        Equal(expected, options.Path);
    }
}
