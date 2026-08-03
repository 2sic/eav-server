namespace ToSic.Eav.Models.Sys;

/// <summary>
/// 
/// </summary>
/// <param name="EntryType"></param>
/// <param name="TrueType"></param>
/// <param name="Options"></param>
internal record ToModelSpecs(Type EntryType, Type TrueType, ToModelOptions Options)
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
        return new(typeof(TModel), trueType, options ?? new());
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
internal record ToModelSpecs<TModel>(bool ExitEarly, TModel? Result, Type TrueType, ToModelOptions Options)
    : ToModelSpecs(typeof(TModel), TrueType, Options)
    where TModel : class, IModelFromEntity
{
    internal static ToModelSpecs<TModel> List(object? list, ToModelOptions? options, Type? trueType, bool useFactory, string methodName)

    {
        var specs = Start<TModel>(trueType, options, useFactory: useFactory, methodName);
        // 2. If Null, exit early
        if (list == null)
            return new(true, /*FromNull<TModel>(trueType, nullHandling: options.NullHandling)*/null, specs.TrueType, specs.Options);

        // 3. If Object not null, continue processing
        return new(false, null, specs.TrueType, specs.Options);
    }

    internal static ToModelSpecs<TModel> Item(object? dataForNullCheck, ToModelOptions? options, Type? trueType, string methodName)
    {
        var specs = Start<TModel>(trueType, options, useFactory: false, methodName);

        // 2. If Null, exit early
        if (dataForNullCheck == null)
            return new(true, ToModelInternal.FromNull<TModel>(specs.TrueType, nullHandling: specs.Options.NullHandling), specs.TrueType, specs.Options);

        // 3. If Object not null, continue processing
        return new(false, null, specs.TrueType, specs.Options);
    }

}