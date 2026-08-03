using System.Runtime.CompilerServices;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

/// <summary>
/// 
/// </summary>
/// <param name="EntryType"></param>
/// <param name="TrueType"></param>
/// <param name="Options"></param>
public record ToModelSpecs(Type EntryType, Type TrueType, ToModelOptions Options, string MethodName)
{
    public static ToModelSpecs Start<TModel>(Type? trueType, ToModelOptions? options, bool useFactory, string methodName)
        where TModel : class, IModelFromEntity
    {
        // 1. Figure out the true type to create, based on implemented interfaces etc.
        // This is important, in case an interface was passed in.
        // If the caller already knows the true type, it can be passed in to avoid the reflection overhead.
        trueType ??= useFactory
            ? ModelFromEntityTypeManager.GetTargetType<TModel>()
            : ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(methodName);

        // 2. Stabilize options and return
        return new(typeof(TModel), trueType, options ?? new(), methodName);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <param name="ExitEarly"></param>
/// <param name="Result"></param>
/// <param name="TrueType"></param>
/// <param name="Options"></param>
/// <param name="MethodName"></param>
internal record ToModelSpecs<TModel>(bool ExitEarly, TModel? Result, Type TrueType, ToModelOptions Options, string MethodName)
    : ToModelSpecs(typeof(TModel), TrueType, Options, MethodName)
    where TModel : class, IModelFromEntity
{
    /// <summary>
    /// 
    /// </summary>
    internal static ToModelSpecs<TModel> List(IEnumerable<IEntity>? list, ToModelOptions? options, Type? trueType, bool useFactory, [CallerMemberName] string? methodName = null)
    {
        var specs = Start<TModel>(trueType, options, useFactory: useFactory, methodName!);
        
        // 2. If Null, exit early
        if (list == null)
            return new(true, null, specs.TrueType, specs.Options, methodName!);

        // 3. If Object not null, continue processing
        return new(false, null, specs.TrueType, specs.Options, methodName!);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static ToModelSpecs<TModel> Item(IEntity? entity, ToModelOptions? options, Type? trueType, string methodName)
    {
        var specs = Start<TModel>(trueType, options, useFactory: false, methodName);

        // 2. If Null, exit early
        if (entity == null)
            return new(true, CreateFromNull(specs), specs.TrueType, specs.Options, methodName);

        // 3. If Object not null, continue processing
        return new(false, null, specs.TrueType, specs.Options, methodName);
    }

    internal ToModelSpecs<TModel> DisableNameCheck()
        => this with { Options = Options with { TypeName = ToModelOptions.TypeNameAny } };

    internal TModel CreateInstance()
        => (TModel)TypeFactory.CreateInstance(TrueType);

    internal static TModel? CreateFromNull(ToModelSpecs specs)
        // Short circuit to avoid creating an instance if null is expected anyhow
        => specs.Options.NullHandling is NullHandling.Default or NullHandling.ReturnNull
            ? default
            : (TypeFactory.CreateInstance(specs.TrueType) as IModelSetup<IEntity>)
            ?.SetupWithNullChecks((IEntity?)null, specs.Options.NullHandling)
            as TModel;
}
