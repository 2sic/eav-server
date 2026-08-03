using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Sys;
// ReSharper disable MethodOverloadWithOptionalParameter

namespace ToSic.Eav.Models;

public static partial class ToModelExtensions
{
    public static IEnumerable<TModel> ToModels<TModel>(this IEnumerable<IEntity>? entities)
        where TModel : class, IModelFromEntity
        => entities?.ToListOpt().ToModelsInternal<TModel>(options: default) ?? [];

    
    
    public static IEnumerable<TModel> ToModels<TModel>(this IEnumerable<IEntity>? entities, NoParamOrder npo = default, ToModelOptions? options = default)
        where TModel : class, IModelFromEntity
        => entities?.ToListOpt().ToModelsInternal<TModel>(options: options) ?? [];

    
    
    /// <summary>
    /// Real implementation of As... methods
    /// </summary>
    /// <typeparam name="TModel">TModel must implement IWrapperSetup&lt;IEntity&gt; and have a parameterless constructor.</typeparam>
    /// <param name="list">The entity to convert.</param>
    /// <param name="methodName">Automatically provided method name for debugging</param>
    /// <param name="options">Conversion options</param>
    /// <returns></returns>
    private static IEnumerable<TModel> ToModelsInternal<TModel>(this IList<IEntity>? list, ToModelOptions? options, [CallerMemberName] string? methodName = default)
        where TModel : class, IModelFromEntity
    {
        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.
        var specs = ToModelSpecs<TModel>.List(list, options: options, null, factory: null, methodName: methodName);
        if (specs.ExitEarly)
            return [];
        
        // Create the model
        var result = list!
            .Select(e => ((IModelSetup<IEntity>)specs.CreateInstance()).Setup(e))
            .OfType<TModel>()
            .ToList();

        return result;
    }

}
