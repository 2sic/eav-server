using System.Runtime.CompilerServices;
using ToSic.Eav.Models;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data;

/// <summary>
/// WIP v21
/// </summary>
[WorkInProgressApi("WIP v21")]
public static partial class EntityExtensions
{
    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullIfNull">If the underlying data is null, prefer null over an empty model.</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? As<TModel>(
        this IEntity? entity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        bool nullIfNull = true
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        return entity.AsInternal<TModel>(skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull);
    }

    /// <summary>
    /// WIP
    /// Convert something which can be an entity to a model of type TModel.
    /// This is only meant for simple models that do not require a factory.
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="canBeEntity"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullIfNull">If the underlying data is null, prefer null over an empty model.</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TModel? As<TModel>(
        this ICanBeEntity? canBeEntity,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        bool nullIfNull = true
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        return (canBeEntity?.Entity).AsInternal<TModel>(skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull);
    }

    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="entity">The entity to convert.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullIfNull">If the underlying data is null, prefer null over an empty model.</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static TModel? AsInternal<TModel>(
        this IEntity? entity,
        [CallerMemberName] string? methodName = default,
        NoParamOrder npo = default,
        bool skipTypeCheck = false,
        bool nullIfNull = true
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.

        if (nullIfNull && entity == null)
            return null;

        // If it is not null, do check if the cast uses the correct type
        if (entity != null)
            DataModelAnalyzer.IsTypeNameAllowedOrThrow<TModel>(entity, entity.EntityId, skipTypeCheck);

        // Create the model
        var wrapper = new TModel();

        // Throw if TModel inherits from INeedsFactory
        if (wrapper is IModelFactoryRequired)
            throw new InvalidCastException($"Cannot cast to '{typeof(TModel)}' because it requires a factory. Use 'SomeFactory.{methodName}<TModel>(...)' instead");

        // Do Setup and check if it's ok.
        // Wrapper will return false if the entity is null or invalid for the model.
        var ok = wrapper.SetupModel(entity);
        return ok ? wrapper : default;
    }





    public static IEnumerable<TModel> AsList<TModel>(
        this IEnumerable<IEntity?> entities,
        NoParamOrder npo = default
        //bool skipTypeCheck = false,
        //bool nullIfNull = false
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        return entities.AsListInternal<TModel>(/*skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull*/);
    }


    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="list">The entity to convert.</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="skipTypeCheck">allow conversion even if the Content-Type of the entity doesn't match the type specified in the parameter T</param>
    /// <param name="nullIfNull">If the underlying data is null, prefer null over an empty model.</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static IEnumerable<TModel> AsListInternal<TModel>(
        this IEnumerable<IEntity?> entities,
        [CallerMemberName] string? methodName = default,
        NoParamOrder npo = default
        //bool skipTypeCheck = true
        //bool nullIfNull = false
    )
        where TModel : class, IModelSetup<IEntity>, new()
    {
        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.

        var list = entities?.ToList();

        if (/*nullIfNull &&*/ list == null || !list.Any())
            return [];

        // If it is not null, do check if the cast uses the correct type
        //if (list != null)
        //    DataModelAnalyzer.IsTypeNameAllowedOrThrow<TModel>(list, "many", skipTypeCheck);

        // Create the model
        var result = list
            .Select(e =>
            {
                var wrapper = new TModel();

                // Throw if TModel inherits from INeedsFactory
                if (wrapper is IModelFactoryRequired)
                    throw new InvalidCastException(
                        $"Cannot cast to '{typeof(TModel)}' because it requires a factory. Use 'SomeFactory.{methodName}<TModel>(...)' instead");

                // Do Setup and check if it's ok.
                // Wrapper will return false if the entity is null or invalid for the model.
                var ok = wrapper.SetupModel(e);
                return ok ? wrapper : default!;
            })
            .Where(m => m != null)
            .ToList();

        return result;
    }

}
