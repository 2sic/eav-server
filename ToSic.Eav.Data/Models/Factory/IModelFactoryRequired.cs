namespace ToSic.Eav.Models.Factory;

/// <summary>
/// WIP - nothing done yet, just as a reminder.
/// Goal is that some wrappers require a factory, and these should be marked as such,
/// so that a simple wrapper helper can detect and warn about this.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IModelFactoryRequired;

/// <summary>
/// WIP so an object can declare that it needs a specific factory.
/// </summary>
/// <typeparam name="TFactory"></typeparam>
public interface IModelFactoryRequired<TFactory>: IModelFactoryRequired;