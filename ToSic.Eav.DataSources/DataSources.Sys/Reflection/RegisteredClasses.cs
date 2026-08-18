using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource.Sys;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Generic Data Source to provide reflection data about classes or interfaces - but only such that are registered in the DI.
/// </summary>
/// <remarks>
/// Used for example to provide the list of IWorkEntityAction implementations, but can be used for any class or interface type.
/// Created in v21.02.
/// </remarks>
/// <typeparam name="TClassOrInterface"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class RegisteredClasses<TClassOrInterface>: CustomDataSource where TClassOrInterface: class
{
    protected RegisteredClasses(Dependencies services, LazySvc<IEnumerable<TClassOrInterface>> servicesOfType)
        : base(services, logName: $"{DataSourceConstantsInternal.LogPrefix}.C#Cls", connect: [servicesOfType])
    {
        ProvideOutRaw(() => Generators(servicesOfType.Value));
    }

    private IEnumerable<IRawData> Generators(IEnumerable<TClassOrInterface> servicesOfType)
    {
        var l = Log.Fn<IEnumerable<IRawData>>();
        var list = servicesOfType
            .Select(g => g.GetType())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select((type, index) => new ClassInfoRaw(type, index))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }


}
