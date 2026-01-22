namespace ToSic.Eav.Data;

/// <summary>
/// WIP v21
/// </summary>
[PublicApi]
public static class EntityExtensions
{
    public static TModel? As<TModel>(this IEntity? entity)
        where TModel : IWrapperSetup<IEntity>, new()
    {
        if (entity == null)
            return default;

        var wrapper = new TModel();
        wrapper.SetupContents(entity);
        return wrapper;
    }

}
