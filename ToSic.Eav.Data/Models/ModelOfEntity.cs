using System.Runtime.CompilerServices;

namespace ToSic.Eav.Models;

/// <summary>
/// Foundation for a *record* which gets its data from an Entity. Completely empty, no public properties.
/// </summary>
/// <remarks>
/// This **Core** implementation has zero public properties, so no public `Id`, `Guid` or `Title` properties.
/// If serialized or use otherwise it will not include anything which was not added explicitly.
///
/// This is the **record** implementation, which is the preferred future way of creating models.
/// To use the **class** based implementation (for example in DNN code not supporting c# 10), use the <see cref="ModelOfEntityClassic"/>
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record ModelOfEntity
    : IModelSetup<IEntity>,     // Allow setting up the wrapper with an entity
        ICanBeEntity            // Allow retrieving the entity directly if needed
{
    #region Constructors & Setup

    /// <summary>
    /// Empty constructor, mainly for factories which must call the setup (otherwise risky to use)
    /// </summary>
    /// <remarks>
    /// This is the primary constructor used by most inheriting classes.
    /// </remarks>
    protected ModelOfEntity() { }

    /// <summary>
    /// Standard constructor providing the entity during construction.
    /// </summary>
    /// <param name="entity">Entity to wrap</param>
    protected ModelOfEntity(IEntity entity)
        => Entity = entity;

    /// <summary>
    /// The underlying entity.
    /// It's explicitly _not_ public, so it won't end up in serializations etc.
    /// So it's only accessible from within the object (protected).
    /// </summary>
    protected IEntity Entity { get; private set; } = null!;

    IEntity ICanBeEntity.Entity => Entity;


    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        Entity = source!;
        return true;
    }

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