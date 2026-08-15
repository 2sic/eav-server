using ToSic.Sys;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;

namespace ToSic.HookUp.Work.DoNamed;

public class DoNamedTests(DoNamed<IWork<string, string>, string> doNamed)
{
    public class Startup() : QuickStartup(s => s.AddSysCoreDi().AddHookUp().AddMockNamedServices());
    
    [Fact]
    public async Task TryDoNamed()
    {
        var x=  await doNamed.Handle(new(), new((MockNamedBefore.PhaseName, "DataValue")));
        Equal("Run before", x.Data);
    }
}
