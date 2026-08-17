using ToSic.Eav.Data.ContentTypes;
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
    protected RegisteredClasses(Dependencies services, LazySvc<IEnumerable<TClassOrInterface>> generators)
        : base(services, logName: $"{DataSourceConstantsInternal.LogPrefix}.C#Cls", connect: [generators])
    {
        ProvideOutRaw(
            () => Generators(generators.Value)
            //options: () => new()
            //{
            //    AutoId = true,
            //    TitleField = "FullName",
            //    TypeName = "Classes",
            //}
            );
    }

    private IEnumerable<IRawData> Generators(IEnumerable<TClassOrInterface> fileGenerators)
    {
        var l = Log.Fn<IEnumerable<IRawData>>();
        var list = fileGenerators
            .Select(g => g.GetType())
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .Select((type, index) => new ClassRaw(index, type.Name, type.FullName ?? "", type.Assembly.FullName, type.AssemblyQualifiedName ?? "", type.Namespace ?? ""))
            //{
            //    Values = new Dictionary<string, object?>
            //    {
            //        { nameof(type.Name), type.Name },
            //        { nameof(type.FullName), type.FullName },
            //        { nameof(type.Assembly), type.Assembly },
            //        { nameof(type.AssemblyQualifiedName), type.AssemblyQualifiedName },
            //        { nameof(type.Namespace), type.Namespace },
            //    }
            //})
            .ToList();

        return l.Return(list, $"{list.Count}");
    }

    [ContentType(
        Name = "Classes",
        Guid = "6eb62f0c-e6b9-414d-999f-24fc972bdf9c")]
    private record ClassRaw(
        int Id,
        string Name,
        [property: ContentTypeTitle]
        string FullName,
        string Assembly,
        string AssemblyQualifiedName,
        string Namespace
    ) : IRawEntityAutoConvert;
}