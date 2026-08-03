using System.Runtime.CompilerServices;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ToModelInternal
{
    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity">The entity to convert.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="options"></param>
    /// <param name="trueType">The true type to actually use, in case the caller already checked for GetTargetType (so it should be reused)</param>
    /// <param name="methodName">Automatically added method name</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static TModel? ToModelOrNull<TModel>(
        this IEntity? entity,
        ToModelOptions options,
        NoParamOrder npo = default,
        Type? trueType = default,
        [CallerMemberName] string? methodName = default
    )
        where TModel : class, IModelFromEntity
    {
        // 1. Do Preflight; stabilize parameters and check if exit early is needed
        var specs = ToModelSpecs<TModel>.Item(entity, options, trueType, methodName!);
        if (specs.ExitEarly)
            return specs.Result;

        // 3. Check if the cast uses the correct type
        var checkName = ModelContentTypeNameAnalyzer.IsTypeNameAllowed(specs, entity!.Type);
        if (!checkName.IsOk)
            throw ModelContentTypeNameAnalyzer.KeyNotFoundMessage(checkName.Names ?? [], entity.Type, entity.EntityId);

        // Create the model. Cast is guaranteed, because the trueType was already checked to be compatible with TModel.
        var instance = specs.CreateInstance();

        // Do Setup and check if it's ok.
        // This may throw an exception if the model is not compatible with the entity, which is expected behavior.
        ((IModelSetup<IEntity>)instance).SetupWithNullChecks(entity, specs.Options.NullHandling);
        return instance;
    }
}