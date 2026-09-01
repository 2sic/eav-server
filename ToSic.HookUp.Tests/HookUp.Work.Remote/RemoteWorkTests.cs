using ToSic.HookUp.MockWork;
using ToSic.Sys;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;

namespace ToSic.HookUp.Work.Remote;

public class RemoteWorkTests(RemoteWork<IWork<string, string>, string, string> remoteWork)
{
    public class Startup() : QuickStartup(s => s
        .AddSysCoreDi()
        .AddHookUp()
        .AddMockNamedServices());

    public const string Input = "DataValue";
    
    
    [Fact]
    public async Task RemoteWork_Before()
    {
        var x=  await remoteWork.Handle(new(), new(new DoNamedInput<string>(MockWorkNamedBefore.PhaseName, Input)));
        Equal(Input + MockWorkNamedBefore.AddOn, x.Data);
    }
    
    [Fact]
    public async Task RemoteWork_After()
    {
        var x=  await remoteWork.Handle(new(), new(new DoNamedInput<string>(MockWorkNamedAfter.PhaseName, Input)));
        Equal(Input + MockWorkNamedAfter.AddOn, x.Data);
    }
    
    [Fact]
    public async Task RemoteWork_InvalidName_ReturnSame()
    {
        var x=  await remoteWork.Handle(new(), new(new DoNamedInput<string>("NotFoundPhase", Input)));
        Equal(Input, x.Data);
    }
    
    [Fact]
    public async Task RemoteWork_InvalidName_ReturnAlternative()
    {
        var x=  await remoteWork.Handle(new(), new(new("NotFoundPhase", Input, "Alternative")));
        Equal("Alternative", x.Data);
    }
}
