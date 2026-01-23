namespace ToSic.Eav.Data;

/// <summary>
/// Interface to mark anything that is an entity or is based on an entity
/// to simplify checking if something can be used
///
/// 2022-06-29 2dm - started this idea, but not completed. ATM doesn't serve a purpose yet
/// </summary>
/// <remarks>
/// * Introduced 2022-06 (2sxc 15)
/// * Used extensively in 2sxc 18/19 for APIs which can accept a wide variety of data types
/// * Made public in docs v21
/// </remarks>
[PrivateApi]
public interface ICanBeEntity
{
    /// <summary>
    /// Property to access the underlying entity.
    /// </summary>
    /// <remarks>
    /// Note that many inheriting objects will implement this _explicitly_,
    /// so the property is not visible on the object,
    /// unless you first cast it to <see cref="ICanBeEntity"/>.
    /// </remarks>
    IEntity Entity { get; }
}