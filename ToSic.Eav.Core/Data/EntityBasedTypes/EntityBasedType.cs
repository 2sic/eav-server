using System.Runtime.CompilerServices;
using ToSic.Eav.Metadata;


namespace ToSic.Eav.Data;

/// <summary>
/// Foundation for a class which gets its data from an Entity. <br/>
/// This is used for more type safety - because some internal objects need entities for data-storage,
/// but when programming they should use typed objects to not accidentally access invalid properties. 
/// </summary>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class EntityBasedType : IEntityBasedType
{
    /// <inheritdoc cref="IEntityWrapper.Entity" />
    public IEntity Entity { get; protected set; }

    [PrivateApi] public IEntity RootContentsForEqualityCheck
        => (Entity as IEntityWrapper)?.RootContentsForEqualityCheck ?? Entity;

    public IEnumerable<IDecorator<IEntity>> Decorators
        => field ??= (Entity as IEntityWrapper)?.Decorators ?? [];

    /// <summary>
    /// Create a EntityBasedType and wrap the entity provided
    /// </summary>
    /// <param name="entity"></param>
    protected EntityBasedType(IEntity entity) => Entity = entity;

    protected EntityBasedType(IEntity entity, string[] languageCodes) : this(entity)
        => LookupLanguages = languageCodes ?? [];

    protected EntityBasedType(IEntity entity, string languageCode) : this(entity)
        => LookupLanguages = languageCode != null ? [languageCode] : [];

    /// <inheritdoc />
    public virtual string Title => field ??= Entity?.GetBestTitle() ?? "";

    /// <inheritdoc />
    public int Id => Entity?.EntityId ?? 0;

    /// <inheritdoc />
    public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;

    /// <inheritdoc />
    public IMetadataOf Metadata => Entity?.Metadata;

    [PrivateApi]
    protected string[] LookupLanguages { get; } = [];


    /// <summary>
    /// Get a value from the underlying entity. 
    /// </summary>
    /// <typeparam name="T">type, should only be string, decimal, bool</typeparam>
    /// <param name="fieldName">field name</param>
    /// <param name="fallback">fallback value</param>
    /// <returns>The value. If the Entity is missing, will return the fallback result. </returns>
    protected T Get<T>(string fieldName, T fallback)
        => Entity == null
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
    protected T GetThis<T>(T fallback, [CallerMemberName] string propertyName = default)
        => Get(propertyName, fallback);

    protected T GetThisIfEntity<T>(T fallback, [CallerMemberName] string propertyName = default)
        => Entity == null ? fallback : GetThis(fallback, propertyName);
}