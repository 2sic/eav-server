using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;
// ReSharper disable MethodOverloadWithOptionalParameter

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(this IEntity? entity)
        where TModel : class, IModelFromEntity
        => entity.ToModelOrNull<TModel>(options: new());

    
    
    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(this IEntity? entity, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => entity.ToModelOrNull<TModel>(options: options ?? new());

    
    
    /// <summary>
    /// WIP
    /// Convert something which can be an entity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="canBeEntity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? ToModel<TModel>(this ICanBeEntity? canBeEntity, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => (canBeEntity?.Entity).ToModelOrNull<TModel>(options: options ?? new());

    

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