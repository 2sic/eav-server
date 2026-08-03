using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ToModelIntern
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
        // 1. Figure out the true type to create, based on implemented interfaces etc.
        // This is important, in case an interface was passed in.
        // If the caller already knows the true type, it can be passed in to avoid the reflection overhead.
        trueType ??= ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(methodName!);

        // 2. If Null, exit early
        if (entity == null)
            return FromNull<TModel>(trueType, nullHandling: options.NullHandling);

        // 3. Check if the cast uses the correct type
        // Priority is
        // 1. Specified in options (could also be `*` to allow any type)
        // 2. Derived names of the interface name (e.g. `IContent` -> `Content`, `ContentBlock`, etc.)
        // 3. 
        var checkName = ModelContentTypeNameAnalyzer.IsTypeNameAllowed(options.TypeName, typeof(TModel), trueType, entity.Type);
        if (!checkName.IsOk)
            throw ModelContentTypeNameAnalyzer.KeyNotFoundMessage(checkName.Names ?? [], entity.Type, entity.EntityId);

        // Create the model. Cast is guaranteed, because the trueType was already checked to be compatible with TModel.
        var instance = (TModel)TypeFactory.CreateInstance(trueType);

        // Do Setup and check if it's ok.
        // Wrapper will return false if the entity is null or invalid for the model.
        var ok = ((IModelSetup<IEntity>)instance).SetupModel(entity);
        return ok || options.NullHandling == NullHandling.ReturnModel
            ? instance
            : options.NullHandling == NullHandling.Throw
                ? throw new ArgumentNullException($"Can't setup model of type {typeof(TModel)} with entity {entity.EntityId}")
                : default;
    }



    internal static TModel? FromNull<TModel>(Type trueType, NullHandling nullHandling)
        where TModel : class
    {
        return (TypeFactory.CreateInstance(trueType) as IModelSetup<IEntity>)
            ?.SetupWithDataNullChecks((IEntity?)null, nullHandling)
            as TModel;

    }

}