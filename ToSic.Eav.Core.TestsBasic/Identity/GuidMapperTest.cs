using System.Diagnostics;

namespace ToSic.Eav.Identity;

public class GuidMapperTest
{
    public string GuidCompressTac(Guid value) => value.GuidCompress();
    public Guid GuidRestoreTac(string value) => Mapper.GuidRestore(value);

    [Fact]
    public void CompressAndUnCompress()
    {
        var data = Guid.NewGuid();
        var compressed = GuidCompressTac(data);
        var uncompressed = GuidRestoreTac(compressed);
        Assert.Equal(data, uncompressed);
    }

    [Fact]
    public void CompressAndUnCompressEmpty()
    {
        var data = new Guid("883668ac-b1fd-47fa-acef-00500e5979fa");
        var compressed = GuidCompressTac(data);
        Trace.WriteLine($"Compressed: {compressed}");
        var uncompressed = GuidRestoreTac(compressed);
        Assert.Equal(data, uncompressed);
    }
}