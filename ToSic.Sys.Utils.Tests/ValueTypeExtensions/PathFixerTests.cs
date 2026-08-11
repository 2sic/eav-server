namespace ToSic.Sys.Utils.Tests.ValueTypeExtensions;

public class PathFixerTests
{
    [Fact]
    public void ToSystemPath_NormalizesBothDirectorySeparators()
    {
        var result = @"2sxc\Tenants/1\Sites/2".ToSystemPath();

        Equal(Path.Combine("2sxc", "Tenants", "1", "Sites", "2"), result);
    }
}
