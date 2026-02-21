using ToSic.Eav.Models.Factory;

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
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
    public static TModel? ToModel<TModel>(this IEntity entity, IModelFactory factory)
        where TModel : class, IModelFromEntity
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        if (entity == null! /* paranoid */)
            return default;

        var wrapper = factory.Create<IEntity, TModel>(entity);
        return wrapper;
    }
}
