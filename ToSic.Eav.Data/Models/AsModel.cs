using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Models;

public static class AsModel
{

    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity">The entity to convert.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullHandling">How to handle nulls during the conversion - default is <see cref="ModelNullHandling.Default"/></param>
    /// <param name="methodName">Automatically added method name</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static TModel? AsInternal<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        ModelNullHandling nullHandling = ModelNullHandling.Undefined,
        [CallerMemberName] string? methodName = default
    )
        where TModel : class, IModelSetup<IEntity> // , new()
    {
        if (nullHandling == ModelNullHandling.Undefined)
            nullHandling = ModelNullHandling.Default;

        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.
        if (entity == null)
        {
            var trueType1 = ModelAnalyseUse.GetTargetType<TModel>();
            return (TypeFactory.CreateInstance(trueType1) as TModel)?.SetupWithDataNullChecks(entity, nullHandling);
        }

        // If it is not null, do check if the cast uses the correct type
        DataModelAnalyzer.IsTypeNameAllowedOrThrow<TModel>(entity, entity.EntityId, skipTypeCheck);

        // Create the model
        var trueType2 = ModelAnalyseUse.GetTargetType<TModel>();
        var wrapper = TypeFactory.CreateInstance(trueType2) as TModel
            ?? throw new InvalidCastException($"Cannot create a {typeof(TModel)} based of the recommended type {trueType2.Name}.");

        // Throw if TModel inherits from INeedsFactory
        if (wrapper is IModelFactoryRequired)
            throw new InvalidCastException($"Cannot cast to '{typeof(TModel)}' because it requires a factory. Use 'SomeFactory.{methodName}<TModel>(...)' instead");

        // Do Setup and check if it's ok.
        // Wrapper will return false if the entity is null or invalid for the model.
        var ok = wrapper.SetupModel(entity);
        return ok
            ? wrapper
            : (nullHandling & ModelNullHandling.ModelAsModelForce) != 0
                ? wrapper
                : (nullHandling & ModelNullHandling.ModelNullThrows) != 0
                    ? throw new InvalidCastException($"Cannot cast to '{typeof(TModel)}' because it requires a factory. Use 'SomeFactory.{methodName}<TModel>(...)' instead")
                    : default;
    }
}