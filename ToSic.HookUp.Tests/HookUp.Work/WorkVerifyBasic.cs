using ToSic.HookUp.Work.Mock;
using ToSic.Sys.HookUp;

namespace ToSic.HookUp.Work;

public class WorkVerifyBasic
{
    [Fact]
    public async Task Work_StringNoOp()
    {
        var work = new MockWorkStringNoOp();
        var result = await work.Handle(new(), "test".ToPackage());
        Equal("test", result.Data);
    }

    [Fact]
    public async Task Work_StringNoOpWithException()
    {
        var work = new MockWorkStringNoOpWithException();
        var result = await work.Handle(new(), "test".ToPackage());
        Null(result.Data); // Check for null string or empty
        Equal(ResultState.Error, result.Decision);
        NotEmpty(result.Exceptions);
        Contains(MockWorkStringNoOpWithException.ErrorMessage, result.Exceptions.Select(e => e.Message));
    }
    
    [Fact]
    public async Task Work_StringAddWorld()
    {
        var work = new MockWorkStringAddWorld();
        var result = await work.Handle(new(), "test".ToPackage());
        Equal("test World", result.Data);
    }
}
