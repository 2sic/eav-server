namespace ToSic.Eav.Models.Factory.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelFactoryShared
{
    [return: NotNullIfNotNull(nameof(item))]
    public static TModel? CreateStatic<TSource, TModel>(IServiceProvider sp, TSource? item, ToModelOptions options)
        where TModel : class, IModelFromEntity
    {
        var wrapper = sp.Build<TModel>();
        // New implementation with null checks
        var result = (wrapper as IModelSetup<TSource>)?.SetupWithNullChecks(item, options.NullHandling);
        return result as TModel;
    }
    
}