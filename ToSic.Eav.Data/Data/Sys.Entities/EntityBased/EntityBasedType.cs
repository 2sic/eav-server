using System.Runtime.CompilerServices;
using ToSic.Eav.Metadata;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace ToSic.Eav.Data.Sys.Entities;

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
    public IEntity Entity { get; private set; } = null!;

    [PrivateApi] public IEntity? RootContentsForEqualityCheck
        => (Entity as IEntityWrapper)?.RootContentsForEqualityCheck ?? Entity;

    [field: AllowNull, MaybeNull]
    public IEnumerable<IDecorator<IEntity>> Decorators
        => field ??= (Entity as IEntityWrapper)?.Decorators ?? [];

    #region Constructors & Setup

    protected EntityBasedType() { }

    void IWrapperSetup<IEntity>.SetupContents(IEntity source)
        => Entity = source;

    /// <summary>
    /// Create a EntityBasedType and wrap the entity provided
    /// </summary>
    /// <param name="entity"></param>
    protected EntityBasedType(IEntity entity)
        => Entity = entity;


    protected EntityBasedType(IEntity entity, string?[] languageCodes) : this(entity)
        => LookupLanguages = languageCodes ?? [];

    #endregion


    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual string Title => field
        ??= Entity?.GetBestTitle() ?? "";

    /// <inheritdoc />
    public int Id => Entity?.EntityId ?? 0;

    /// <inheritdoc />
    public Guid Guid => Entity?.EntityGuid ?? Guid.Empty;

    /// <inheritdoc />
#pragma warning disable CS8603 // Possible null reference return.
    public IMetadata Metadata => Entity?.Metadata;
#pragma warning restore CS8603 // Possible null reference return.

    [PrivateApi]
    protected string?[] LookupLanguages { get; } = [];


    /// <summary>
    /// Get a value from the underlying entity. 
    /// </summary>
    /// <typeparam name="T">type, should only be string, decimal, bool</typeparam>
    /// <param name="fieldName">field name</param>
    /// <param name="fallback">fallback value</param>
    /// <returns>The value. If the Entity is missing, will return the fallback result. </returns>
    [return: NotNullIfNotNull(nameof(fallback))]
    protected T? Get<T>(string fieldName, T? fallback)
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
    [return: NotNullIfNotNull(nameof(fallback))]
    protected T? GetThis<T>(T? fallback, [CallerMemberName] string propertyName = default!)
        => Get(propertyName, fallback);

    [return: NotNullIfNotNull(nameof(fallback))]
    protected T? GetThisIfEntity<T>(T fallback, [CallerMemberName] string propertyName = default!)
        => Entity == null ? fallback : GetThis(fallback, propertyName);
}