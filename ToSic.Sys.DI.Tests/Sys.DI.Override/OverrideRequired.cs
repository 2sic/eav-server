using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

public class OverrideRequired(IServiceProvider sp)
{
    #region Classes and interfaces to use

    /// <summary>
    /// This is the interface we must look for, so we can register it separately from the proper implementation.
    /// </summary>
    private interface ICarInitiallyUndefined
    {
        int Value { get; set; }
    }

    private class ICarFord : ICarInitiallyUndefined
    {
        private const int InitialValue = 20395;
        public int Value { get; set; } = InitialValue;
    }

    private class CarUndefinedWillThrow : ICarFord
    {
        public CarUndefinedWillThrow()
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
        services.TryAddTransient(OverrideService<ICarInitiallyUndefined>.Register(() => new CarUndefinedWillThrow()));
        services.TryAddTransient<ICarFord>();  // make sure the underlying real implementation can be resolved
    });
    
    
    
    #region Type which must be overriden; throws in the original implementation
    
    [Fact]
    public void TypeWhichMustBeOverridden_NoOverride_Throws()
        => Throws<NotSupportedException>(sp.GetService<ICarInitiallyUndefined>);


    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithType_NotNull()
    {
        using (OverrideService<ICarInitiallyUndefined>.Use<ICarFord>())
        {
            NotNull(sp.GetService<ICarInitiallyUndefined>());
        }
    }
    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithType_IsNewType()
    {
        using (OverrideService<ICarInitiallyUndefined>.Use<ICarFord>())
        {
            IsType<ICarFord>(sp.GetService<ICarInitiallyUndefined>());
        }
    }

    [Fact]
    public void TypeWhichMustBeOverridden_ReplacedWithValue_IsNewType()
    {
        using (OverrideService<ICarInitiallyUndefined>.Use(_ => new ICarFord()))
        {
            IsType<ICarFord>(sp.GetService<ICarInitiallyUndefined>());
        }
    }

    #endregion    
}