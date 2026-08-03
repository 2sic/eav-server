using System.Runtime.CompilerServices;
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
        => ToModelInternal<TModel>(entity, options: new());

    
    
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
        => ToModelInternal<TModel>(entity, options: options ?? new());

    
    
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
        => ToModelInternal<TModel>(canBeEntity?.Entity, options: options ?? new());



    /// <summary>
    /// WIP, v21
    /// requiring factory...
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="entity"></param>
    /// <param name="factory"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options">Conversion options</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TModel? ToModel<TModel>(this IEntity entity, IModelFactory factory, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => ToModelInternal<TModel>(entity, options: options ?? new(), factory: AssertFactory(factory));

    
    
    /// <summary>
    /// Real implementation of As... methods - will return the model or null
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity">The entity to convert.</param>
    /// <param name="options"></param>
    /// <param name="trueType">The true type to actually use, in case the caller already checked for GetTargetType (so it should be reused)</param>
    /// <param name="factory">The factory to use for creating the model.</param>
    /// <param name="methodName">Automatically added method name</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static TModel? ToModelInternal<TModel>(IEntity? entity, ToModelOptions options, Type? trueType = default, IModelFactory? factory = default, [CallerMemberName] string? methodName = default)
        where TModel : class, IModelFromEntity
    {
        // 1. Do Preflight; stabilize parameters and check if exit early is needed
        var specs = ToModelSpecs<TModel>.Item(entity, options, trueType, factory, methodName!);
        if (specs.ExitEarly)
            return specs.Result;

        // 2. Check if the cast uses the correct type
        var checkName = ModelContentTypeNameAnalyzer.IsTypeNameAllowed(specs, entity!.Type);
        if (!checkName.IsOk)
            throw ModelContentTypeNameAnalyzer.KeyNotFoundMessage(checkName.Names ?? [], entity.Type, entity.EntityId);

        // 3a. If we have a factory, use it to create everything.
        if (factory != null)
            return factory.Create<IEntity, TModel>(entity, options);
        
        // 3b. Create the model.
        // Cast is guaranteed, because the trueType was already checked to be compatible with TModel.
        // Do Setup and check if it's ok. This may throw an exception if the model is not compatible with the entity, which is expected behavior.
        var instance = specs.CreateInstance();
        var result = ((IModelSetup<IEntity>)instance).SetupWithNullChecks(entity, specs.Options.NullHandling);
        return result as TModel;
    }

}