using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.Utils.Types;

public class BuildByNameTests(IServiceProvider sp)
{
    private interface IMockClass;
    private class MockClass : IMockClass;

    private interface IMockWrong;

    public class Startup() : QuickStartup(sc => sc.TryAddTransient<MockClass>());
    
    private const string MockClassFullName = "ToSic.Sys.Utils.Types.BuildByNameTests+MockClass";
    
    [Fact]
    public void BuildByName_Class_NotNull()
        => NotNull(sp.BuildByName<MockClass>(MockClassFullName).Instance);

    [Fact]
    public void BuildByName_Class_InstanceTypeMatches()
        => IsType<MockClass>(sp.BuildByName<MockClass>(MockClassFullName).Instance);
    
    [Fact]
    public void BuildByName_Interface_NotNull()
        => NotNull(sp.BuildByName<IMockClass>(MockClassFullName).Instance);

    [Fact]
    public void BuildByName_Interface_InstanceTypeMatches()
        => IsType<MockClass>(sp.BuildByName<MockClass>(MockClassFullName).Instance);
    
    [Fact]
    public void BuildByName_WrongName_Empty_Null()
        => Null(sp.BuildByName<MockClass>("").Instance);
    [Fact]
    public void BuildByName_WrongName_Null_Null()
        => Null(sp.BuildByName<MockClass>(null!).Instance);
    
    [Fact]
    public void BuildByName_WrongName_Class_Null()
        => Null(sp.BuildByName<MockClass>("wrong").Instance);
    
    [Fact]
    public void BuildByName_WrongName_Interface_Null()
        => Null(sp.BuildByName<IMockClass>("wrong").Instance);

    [Fact]
    public void BuildByName_Interface_WrongInterface_NotNull()
        => Null(sp.BuildByName<IMockWrong>(MockClassFullName).Instance);
}
