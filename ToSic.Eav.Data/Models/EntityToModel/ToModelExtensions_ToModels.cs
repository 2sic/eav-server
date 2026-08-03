using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.Sys;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    public static IEnumerable<TModel> ToModels<TModel>(
        this IEnumerable<IEntity?> entities,
        NoParamOrder npo = default
    )
        where TModel : class, IModelFromEntity, new()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        return entities?.ToListOpt()?.ToModelsInternal<TModel>() ?? [];
    }


    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="list">The entity to convert.</param>
    /// <param name="methodName">Automatically provided method name for debugging</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    internal static IEnumerable<TModel> ToModelsInternal<TModel>(
        this IList<IEntity?> list,
        [CallerMemberName] string? methodName = default,
        NoParamOrder npo = default
    )
        where TModel : class, IModelFromEntity
    {
        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.

        if (list.SafeNone())
            return [];

        // 1. Figure out the true type to create, based on implemented interfaces etc.
        // This is important, in case an interface was passed in.
        // If the caller already knows the true type, it can be passed in to avoid the reflection overhead.
        var trueType = ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(methodName!);

        // If it is not null, do check if the cast uses the correct type
        //if (list != null)
        //    DataModelAnalyzer.IsTypeNameAllowedOrThrow<TModel>(list, "many", skipTypeCheck);

        //var wrapper = (TModel)TypeFactory.CreateInstance(trueType);// as TModel
                      //?? throw ToModelIntern.InvalidCastException<TModel>(trueType);

        //// Throw if TModel inherits from INeedsFactory
        //if (wrapper is IModelFactoryRequired)
        //    throw ToModelIntern.RequiresFactoryException<TModel>(methodName);
        

        // Create the model
        var result = list
            .Select(e =>
            {
                var wrapper = (TModel)TypeFactory.CreateInstance(trueType);
                
                // Do Setup and check if it's ok.
                // Wrapper will return false if the entity is null or invalid for the model.
                var ok = ((IModelSetup<IEntity>)wrapper).SetupModel(e);
                return ok ? wrapper : default!;
            })
            .Where(m => m != null)
            .ToList();

        return result;
    }

}
