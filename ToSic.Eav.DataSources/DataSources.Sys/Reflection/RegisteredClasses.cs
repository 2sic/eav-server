using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.DataSource.Sys;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Generic data source to provide reflection data about classes or interfaces.
/// Used for example to provide the list of IDataProcessor implementations, but can be used for any class or interface type.
/// </summary>
/// <typeparam name="TClassOrInterface"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class RegisteredClasses<TClassOrInterface>: CustomDataSource where TClassOrInterface: class
{
    protected RegisteredClasses(Dependencies services, LazySvc<IEnumerable<TClassOrInterface>> generators)
        : base(services, logName: $"{DataSourceConstantsInternal.LogPrefix}.C#Cls", connect: [generators])
    {
        ProvideOutRaw(
            () => Generators(generators.Value),
            options: () => new()
            {
                AutoId = true,
                TitleField = "FullName",
                TypeName = "Classes",
            });
    }

    private IEnumerable<IRawEntity> Generators(IEnumerable<TClassOrInterface> fileGenerators)
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        var list = fileGenerators
            .Select(g => g.GetType())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select(type => new RawEntity(new()
            {
                //{ AttributeNames.NameIdNiceName, g.NameId },
                { nameof(type.Name), type.Name },
                { nameof(type.FullName), type.FullName },
                { nameof(type.Assembly), type.Assembly },
                { nameof(type.AssemblyQualifiedName), type.AssemblyQualifiedName },
                { nameof(type.Namespace), type.Namespace },
            }))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }


}