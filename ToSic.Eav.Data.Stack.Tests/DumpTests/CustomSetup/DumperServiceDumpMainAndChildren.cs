using ToSic.Eav.Data.PropertyDump.Sys;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;
public class DumperServiceDumpMainAndChildren(IPropertyDumpService dumperService)
{
    [Fact]
    public void CanDumpTrivial()
    {
        // Arrange
        var main = new ObjectMain();
        var dumped = dumperService.Dump(main, new("Entry"), "");
        NotNull(dumped);
        Equal(3, dumped.Count);
        Equal(ObjectMainDumper.PathToUse, dumped.First().Path);
        Equal(ObjectChildDumper.PathToUse, dumped.Last().Path);
    }
}
