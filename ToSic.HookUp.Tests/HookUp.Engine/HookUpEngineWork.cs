using ToSic.HookUp.MockWork;
using ToSic.Sys;
using ToSic.Sys.HookUp;
using ToSic.Sys.Run.Startup;

namespace ToSic.HookUp.Engine;

public class HookUpEngineWork(IHookUp hookUp, MockWorkStringAddWorld workAddWorld, MockWorkStringLength workStringLength)
{
    public class Startup() : QuickStartup(s => s
        .AddHookUp()
        .AddMockNamedServices());

    [Fact]
    public async Task Work_Generic_SameResultType_Auto()
    {
        var x = await hookUp
            .StartWith("test")
            .Work<MockWorkStringAddWorld>();
        
        Equal("testWorld", x.Package.Data);
    }
    
    [Fact]
    public async Task Work_Generic_SameResultType_Manual()
    {
        var x = await hookUp
            .StartWith("abc")
            .Work<MockWorkStringAddWorld, string>();
        
        Equal("abcWorld", x.Package.Data);
    }
    
    [Fact]
    public async Task Work_IWork_SameResultType_Auto()
    {
        var x = await hookUp
            .StartWith("abc")
            .Work(workAddWorld);
        
        Equal("abcWorld", x.Package.Data);
    }
    
    [Fact]
    public async Task Work_IWork_DiffResultType_Auto()
    {
        var x = await hookUp
            .StartWith("question")
            .Work(workStringLength);
        
        Equal(8, x.Package.Data);
    }
}