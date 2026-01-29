namespace ToSic.Eav.Models.Factory;

/// <summary>
/// WIP - nothing done yet, just as a reminder.
/// Goal is that some wrappers require a factory, and these should be marked as such,
/// so that a simple wrapper helper can detect and warn about this.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IModelFactory
{
    /// <summary>
    /// Creates a new instance of the specified wrapper model type and initializes it using the provided source object.
    /// </summary>
    /// <remarks>
    /// The created model is set up using the <see cref="IModelSetup{TSource}"/> interface, which
    /// allows custom initialization logic based on the source object.
    /// </remarks>
    /// <typeparam name="TSource">The type of the source object used to initialize the wrapper model.</typeparam>
    /// <typeparam name="TModel">The type of the wrapper model to create. Must implement <see cref="IModelSetup{TSource}"/>.</typeparam>
    /// <param name="source">The source object containing data used to set up the wrapper model. Cannot be null.</param>
    /// <returns>An instance of <typeparamref name="TModel"/> initialized with the specified source object.</returns>
    public TModel? Create<TSource, TModel>(TSource? source) where TModel : IModelSetup<TSource>;
}
