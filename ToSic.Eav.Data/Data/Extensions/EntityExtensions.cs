namespace ToSic.Eav.Data;

/// <summary>
/// WIP v21
/// </summary>
[WorkInProgressApi("WIP v21")]
public static class EntityExtensions
{
    public static TModel? As<TModel>(this IEntity? entity)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        // Note: No early null-check, as each model can decide if it's valid or not
        // and the caller could always do a ?.As<TModel>() anyway.

        // Create the model
        var wrapper = new TModel();

        // Throw if TModel inherits from INeedsFactory
        if (wrapper is INeedsFactory)
            throw new InvalidCastException($"Cannot cast to {typeof(TModel)} because it requires a factory.");

        // Do Setup and check if it's ok
        var ok = wrapper.SetupContents(entity);
        return ok ? wrapper : default;
    }

    public static TModel? As<TModel>(this IEntity? entity, IWrapperFactory factory)
        where TModel : IWrapperSetup<IEntity>
    {
        if (entity == null)
            return default;

        var wrapper = factory.Create<IEntity, TModel>(entity);
        return wrapper;
    }

}
