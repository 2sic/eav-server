using ToSic.Eav.Data.PropertyDump.Sys;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;
public class DumperServiceCanDumpTrivial(IPropertyDumpService dumperService)
{
    [Fact]
    public void CanDumpTrivial()
    {
        // Arrange
        var main = new ObjectTrivial();
        var dumped = dumperService.Dump(main, new("Entry"), "");
        NotNull(dumped);
        Single(dumped);
        Equal(ObjectTrivialDumper.PathToUse, dumped.First().Path);
    }
}
