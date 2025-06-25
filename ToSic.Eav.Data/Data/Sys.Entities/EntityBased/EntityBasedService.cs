namespace ToSic.Eav.Data.Sys.Entities;

/// <summary>
/// Experimental type for a thing which needs services to work, but has all the data from a Entity.
/// </summary>
[PrivateApi("WIP v15")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class EntityBasedService(string logName) : EntityBasedWithLog(null!, null, logName)
{
    internal void InitInternal(IEntity entity) => Entity = entity;
}

public static class EntityBasedServiceExtensions
{
    /// <summary>
    /// Initializes the service with the entity.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="service">The service to initialize.</param>
    /// <param name="entity">The entity to use for initialization.</param>
    /// <returns>The initialized service.</returns>
    public static T Init<T>(this T service, IEntity entity) where T : EntityBasedService
    {
        service.InitInternal(entity);
        return service;
    }
}