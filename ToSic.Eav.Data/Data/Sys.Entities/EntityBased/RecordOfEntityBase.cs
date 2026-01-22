using System.Runtime.CompilerServices;


namespace ToSic.Eav.Data.Sys.Entities;

/// <summary>
/// Foundation for a class which gets its data from an Entity. <br/>
/// This is used for more type safety - because some internal objects need entities for data-storage,
/// but when programming they should use typed objects to not accidentally access invalid properties. 
/// </summary>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record RecordOfEntityBase
    : IWrapperSetup<IEntity>,   // Allow setting up the wrapper with an entity
        IWrapper<IEntity>,      // Make sure it can be seen as an entity wrapper
        ICanBeEntity            // Allow retrieving the entity directly if needed
{
    #region Constructors & Setup

    /// <summary>
    /// Empty constructor, mainly for factories which will call the setup (otherwise risky to use)
    /// </summary>
    protected RecordOfEntityBase() { }

    /// <summary>
    /// Standard constructor providing the entity.
    /// </summary>
    /// <param name="entity">Entity to wrap</param>
    protected RecordOfEntityBase(IEntity entity)
        => Entity = entity;

    /// <inheritdoc cref="IEntityWrapper.Entity" />
    protected IEntity Entity { get; private set; } = null!;

    IEntity ICanBeEntity.Entity => Entity;


    void IWrapperSetup<IEntity>.SetupContents(IEntity source)
        => Entity = source;

    IEntity IWrapper<IEntity>.GetContents() => Entity;

    #endregion

    /// <summary>
    /// Language codes for value lookups.
    /// If an inheriting class needs to support it, it must set it in its constructor.
    /// </summary>
    [PrivateApi]
    protected string?[] LookupLanguages { get; init; } = [];


    /// <summary>
    /// Get a value from the underlying entity. 
    /// </summary>
    /// <typeparam name="T">type, should only be string, decimal, bool</typeparam>
    /// <param name="fieldName">field name</param>
    /// <param name="fallback">fallback value</param>
    /// <returns>The value. If the Entity is missing, will return the fallback result. </returns>
    [return: NotNullIfNotNull(nameof(fallback))]
    protected T? Get<T>(string fieldName, T? fallback)
        => Entity == null!
            ? fallback
            : Entity.Get(fieldName, fallback: fallback, languages: LookupLanguages);

    /// <summary>
    /// Get a value from the underlying entity, whose name matches the property requesting this.
    /// So if your C# property is called `Birthday` it will also get the field `Birthday` in the entity.
    /// </summary>
    /// <typeparam name="T">Optional type, usually auto-detected because of the `fallback` value</typeparam>
    /// <param name="fallback">Value to provide if nothing was found - required</param>
    /// <param name="propertyName">The property name - will be auto-filled by the compiler</param>
    /// <returns>The typed value</returns>
    [return: NotNullIfNotNull(nameof(fallback))]
    protected T? GetThis<T>(T? fallback, [CallerMemberName] string propertyName = default!)
        => Get(propertyName, fallback);
}