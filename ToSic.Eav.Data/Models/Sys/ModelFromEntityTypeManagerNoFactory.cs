using System.Collections.Concurrent;
using ToSic.Eav.Models.Factory;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

/// <summary>
/// Manages models which should not require a factory.
/// </summary>
/// <remarks>
/// Performs a set of compliance checks to verify that the type can be used in a standalone/no-factory scenario.
/// If it fails, the exception will be cached and rethrown on every access,
/// to avoid running the identical checks again and again.
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ModelFromEntityTypeManagerNoFactory
{
    /// <summary>
    /// Record to store the cached information.
    /// </summary>
    /// <param name="Type">A lazy type - will also preserve the exception and rethrow if accessed.</param>
    /// <param name="Status">The status of the model creation.</param>
    /// <param name="Exception">The exception encountered during model creation, if any.</param>
    private record TypeStorage(Lazy<Type> Type, ModelCreationStatus Status, Exception? Exception)
    {
        public TypeStorage(Exception ex, ModelCreationStatus status)
            : this(new(() => throw ex), status, ex)
        { }
    };

    /// <summary>
    /// Cache
    /// </summary>
    private static readonly ConcurrentDictionary<Type, TypeStorage> TargetTypesCache = new();

    /// <summary>
    /// Public accessor. Will cache the result of the checks, and rethrow any exceptions if the type is not compatible.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static Type GetTargetType<TModel>(string methodName) where TModel : class, IModelFromEntity
        => TargetTypesCache
            .GetOrAdd(typeof(TModel), _ => FindTargetTypeNoFactory<TModel>(methodName))
            .Type.Value;    // This will re-throw the exception if it was stored in the TypeStorage

    /// <summary>
    /// Internal method to find the target type and perform the necessary checks.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="methodName"></param>
    /// <returns></returns>
    private static TypeStorage FindTargetTypeNoFactory<TModel>(string methodName) where TModel : class, IModelFromEntity
    {
        try
        {
            var trueType = ModelFromEntityTypeManager.GetTargetType<TModel>();

            // Test-Create the type to determine if it's compatible
            // Cache / remember exceptions raised during normal creation.
            object instanceRaw;
            try
            {
                instanceRaw = TypeFactory.CreateInstance(trueType);
            }
            catch (Exception ex)
            {
                return new(ex, ModelCreationStatus.ErrorCreateInstance);
            }

            // Verify it can be cast to the specified model type
            if (instanceRaw is not TModel instance)
                return new(InvalidCastException<TModel>(trueType), ModelCreationStatus.InvalidCast);

            // Verify it doesn't require a factory
            if (instance is IModelFactoryRequired)
                return new(RequiresFactoryException<TModel>(methodName), ModelCreationStatus.RequiresFactory);

            // Verify it implements the IModelSetup interface for IEntity
            if (instance is not IModelSetup<IEntity>)
                return new(MissingSetupException<TModel>(trueType), ModelCreationStatus.MissingSetup);

            return new(new(() => trueType), ModelCreationStatus.Success, null);
        }
        catch (Exception ex)
        {
            return new(ex, ModelCreationStatus.UnknownError);
        }
    }


    private static InvalidCastException InvalidCastException<TModel>(Type trueType)
        => new($"Cannot convert {trueType.Namespace}.{trueType.Name} to {typeof(TModel)} - seems to be incompatible.");
    private static MissingSetupException MissingSetupException<TModel>(Type trueType)
        => new($"Cannot convert {trueType.Namespace}.{trueType.Name} to {typeof(IModelSetup<IEntity>)} - seems to be incompatible.");

    private static MissingFactoryException RequiresFactoryException<TModel>(string? methodName) => new(
        $"""
         Cannot cast to '{typeof(TModel)}' because it says it requires a factory.
         This is usually because the model has more advanced features.
         Please use '.{methodName}<TModel>(..., factory: modelFactory)' or the appropriate create method on a factory, such as 'As<{typeof(TModel)}>().
         """
    );
}