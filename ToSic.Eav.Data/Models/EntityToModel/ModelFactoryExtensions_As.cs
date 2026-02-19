using ToSic.Eav.Models.Factory;

namespace ToSic.Eav.Models;

/// <summary>
/// WIP v21
/// </summary>
[WorkInProgressApi("WIP v21")]
public static partial class ModelFactoryExtensions
{
    /// <summary>
    /// WIP, v21
    /// requiring factory...
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="entity"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TModel? As<TModel>(this IModelFactory factory, IEntity? entity)
        where TModel : class, IModelSetup<IEntity>
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        if (entity == null)
            return default;

        var wrapper = factory.Create<IEntity, TModel>(entity);
        return wrapper;
    }
}
