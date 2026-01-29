using System.Runtime.CompilerServices;
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
        bool nullIfNull = false
    )
        where TModel : class, IWrapperSetup<IEntity>, new() =>
        entity.AsInternal<TModel>(skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull);

    /// <summary>
    /// WIP
    /// Convert an IEntity to a model of type TModel.
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
        bool nullIfNull = false
    )
        where TModel : class, IWrapperSetup<IEntity>, new() =>
        (canBeEntity?.Entity).AsInternal<TModel>(skipTypeCheck: skipTypeCheck, nullIfNull: nullIfNull);

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
        bool nullIfNull = false
    )
        where TModel : class, IWrapperSetup<IEntity>, new()
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
        if (wrapper is INeedsFactory)
            throw new InvalidCastException($"Cannot cast to '{typeof(TModel)}' because it requires a factory. Use 'SomeFactory.{methodName}<TModel>(...)' instead");

        // Do Setup and check if it's ok.
        // Wrapper will return false if the entity is null or invalid for the model.
        var ok = wrapper.SetupContents(entity);
        return ok ? wrapper : default;
    }

}
