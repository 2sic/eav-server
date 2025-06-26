using Xunit.Abstractions;

namespace ToSic.Eav.Identity;

public class GuidMapperTest(ITestOutputHelper output)
{
    public string GuidCompressTac(Guid value) => value.GuidCompress();
    public Guid GuidRestoreTac(string value) => Mapper.GuidRestore(value);

    [Fact]
    public void CompressAndUnCompress()
    {
        var data = Guid.NewGuid();
        var compressed = GuidCompressTac(data);
        var uncompressed = GuidRestoreTac(compressed);
        Equal(data, uncompressed);
    }

    [Fact]
    public void CompressAndUnCompressEmpty()
    {
        var data = new Guid("883668ac-b1fd-47fa-acef-00500e5979fa");
        var compressed = GuidCompressTac(data);
        output.WriteLine($"Compressed: {compressed}");
        var uncompressed = GuidRestoreTac(compressed);
        Equal(data, uncompressed);
    }
}