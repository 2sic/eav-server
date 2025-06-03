using ToSic.Eav.Data.PropertyDump.Sys;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;
public class DumperServiceCanFindRegisteredDumper(IPropertyDumpService dumperService)
{
    [Fact]
    public void CanFindObjectMainDumper()
    {
        var dumper = dumperService.GetBestDumper(new ObjectMain());
        NotNull(dumper.Dumper);
        Equal(ObjectMainDumper.Ranking, dumper.Ranking);
    }

    [Fact]
    public void CanFindObjectTrivialDumper()
    {
        var dumper = dumperService.GetBestDumper(new ObjectTrivial());
        NotNull(dumper.Dumper);
        Equal(ObjectTrivialDumper.Ranking, dumper.Ranking);
    }

    [Fact]
    public void CanNotFindObjectWithoutDumper()
    {
        var dumper = dumperService.GetBestDumper(new ObjectWithoutDumper());
        Null(dumper.Dumper);
        Equal(0, dumper.Ranking);
    }
}
