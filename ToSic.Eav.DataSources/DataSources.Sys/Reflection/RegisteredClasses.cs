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
public abstract class RegisteredClasses<TClassOrInterface>(
    CustomDataSource.Dependencies services,
    LazySvc<IEnumerable<TClassOrInterface>> servicesOfType)
    : CustomDataSource(services, logName: $"{DataSourceConstantsInternal.LogPrefix}.C#Cls", connect: [servicesOfType])
    where TClassOrInterface: class
{
    protected override IEnumerable<IRawData> GetDefault()
    {
        var l = Log.Fn<IEnumerable<IRawData>>();
        var list = servicesOfType.Value
            .Select(g => g.GetType())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select((type, index) => new ClassInfoRaw(type, index))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }


}
