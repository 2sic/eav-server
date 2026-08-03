using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Sys;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models;

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
        // 1. Figure out the true type to create, based on implemented interfaces etc.
        // This is important, in case an interface was passed in.
        // If the caller already knows the true type, it can be passed in to avoid the reflection overhead.
        trueType ??= ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(methodName!);

        // 2. If Null, exit early
        if (entity == null)
            return FromNull<TModel>(trueType, nullHandling: options.NullHandling);

        // 3. Check if the cast uses the correct type
        var checkName = ModelContentTypeNameAnalyzer.IsTypeNameAllowed(options.TypeName, typeof(TModel), trueType, entity.Type);
        if (!checkName.IsOk)
            throw ModelContentTypeNameAnalyzer.KeyNotFoundMessage(checkName.Names ?? [], entity.Type, entity.EntityId);

        // Create the model. Cast is guaranteed, because the trueType was already checked to be compatible with TModel.
        var instance = (TModel)TypeFactory.CreateInstance(trueType);

        // Do Setup and check if it's ok.
        // This may throw an exception if the model is not compatible with the entity, which is expected behavior.
        ((IModelSetup<IEntity>)instance).SetupWithNullChecks(entity, options.NullHandling);
        return instance;
    }



    internal static TModel? FromNull<TModel>(Type trueType, NullHandling nullHandling)
        where TModel : class
        // Short circuit to avoid creating an instance if null is expected anyhow
        => nullHandling is NullHandling.Default or NullHandling.ReturnNull
            ? default
            : (TypeFactory.CreateInstance(trueType) as IModelSetup<IEntity>)
            ?.SetupWithNullChecks((IEntity?)null, nullHandling)
            as TModel;
}