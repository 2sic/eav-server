using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using ToSic.Eav.Identity;

namespace ToSic.Eav.Core.Tests.IdentityTests;

[TestClass]
public class MapperTest
{
    public string TacGuidCompress(Guid value) => Mapper.GuidCompress(value);
    public Guid TacGuidRestore(string value) => Mapper.GuidRestore(value);

    [TestMethod]
    public void CompressAndUnCompress()
    {
        var data = Guid.NewGuid();
        var compressed = TacGuidCompress(data);
        var uncompressed = TacGuidRestore(compressed);
        Assert.AreEqual(data, uncompressed);
    }

    [TestMethod]
    public void CompressAndUnCompressEmpty()
    {
        var data = new Guid("8aa5315d-56b4-4b0b-8a27-8843324bd4fe");
        var compressed = TacGuidCompress(data);
        Trace.WriteLine($"Compressed: {compressed}");
        var uncompressed = TacGuidRestore(compressed);
        Assert.AreEqual(data, uncompressed);
    }
}