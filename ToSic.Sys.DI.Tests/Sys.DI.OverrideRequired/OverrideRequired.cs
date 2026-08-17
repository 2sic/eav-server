using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Mock.LifetimeServices;

namespace ToSic.Sys.DI.OverrideRequired;

public class OverrideRequired(IServiceProvider sp)
{
    #region Classes and interfaces to use

    /// <summary>
    /// This is the interface we must look for, so we can register it separately from the proper implementation.
    /// </summary>
    private interface IMockMustOverride
    {
        int Value { get; set; }
    }

    private class MockMustOverrideOk : IMockMustOverride
    {
        public const int InitialValue = 20395;
        public int Value { get; set; } = InitialValue;
    }

    private class MockMustOverrideWillThrow : MockMustOverrideOk
    {
        public MockMustOverrideWillThrow()
        {
            throw new NotSupportedException();
        }
    }

    #endregion

    /// <summary>
    /// Startup
    /// </summary>
    public class Startup() : QuickStartup(services =>
    {
        // Register the MockChildScopeOnlyTransientPreRegistered as a transient service
        // we'll later access it through the interface, but
        services.TryAddTransient(OverrideService<IMockMustOverride>.Register(() => new MockMustOverrideWillThrow()));
        services.TryAddTransient<MockMustOverrideOk>();  // make sure the underlying real implementation can be resolved
    });
    
    
    
    #region Type which must be overriden; throws in the original implementation
    
    [Fact]
    public void TypeWhichMustBeOverridden_NoOverride_Throws()
        => Throws<NotSupportedException>(sp.GetService<IMockMustOverride>);


    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithType_NotNull()
    {
        using (OverrideService<IMockMustOverride>.Use<MockMustOverrideOk>())
        {
            NotNull(sp.GetService<IMockMustOverride>());
        }
    }
    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithType_IsNewType()
    {
        using (OverrideService<IMockMustOverride>.Use<MockMustOverrideOk>())
        {
            IsType<MockMustOverrideOk>(sp.GetService<IMockMustOverride>());
        }
    }

    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithValue_IsNewType()
    {
        using (OverrideService<IMockMustOverride>.Use(_ => new MockMustOverrideOk()))
        {
            IsType<MockMustOverrideOk>(sp.GetService<IMockMustOverride>());
        }
    }

    #endregion    
}