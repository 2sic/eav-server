// ReSharper disable UnusedMember.Global

namespace ToSic.Eav.DataSource.VisualQuery;

/// <summary>
/// Marks a DataSource to be for a specific audience - `Default` or `Advanced`.
/// Used to hide advanced data sources in the Visual Query editor.
/// </summary>
[PublicApi]
public enum Audience
{
    /// <summary>
    /// Audience not defined, avoid using
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    Undefined = 0,

    /// <summary>
    /// Everyone - ATM not in use, avoid using for now
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    Everyone = 10,

    /// <summary>
    /// Default audience - nothing special configured
    /// </summary>
    Default = 100,

    /// <summary>
    /// Advanced audience - this will be hidden to most normal users in Visual Query
    /// </summary>
    Advanced = 200,

    /// <summary>
    /// Admins only - ATM not in use, avoid using for now.
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    Admin = 1000,

    /// <summary>
    /// System level only - ATM not in use, avoid using for now
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    System = 10000
}