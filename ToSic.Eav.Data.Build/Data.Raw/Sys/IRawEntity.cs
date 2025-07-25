﻿namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// This marks objects which have data prepared to be converted to Entities.
///
/// Typically used for external data DataSources which get something and pass it to an Entity Builder.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("Was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRawEntity
{
    /// <summary>
    /// The ID to use.
    /// If the real object doesn't have a real ID, please do not set at all.
    /// It will then keep the default and will auto-enumerate.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The Guid to use. Must always be set.
    /// If you don't have a GUID, use Guid.Empty
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Created - either the real creation date or the DateTime.Now
    /// </summary>
    public DateTime Created { get; }

    /// <summary>
    /// Modified - either the real creation date or the DateTime.Now
    /// </summary>
    public DateTime Modified { get; }

    /// <summary>
    /// Dictionary of all values to be added.
    /// </summary>
    /// <remarks>
    /// * Please ensure it doesn't have duplicate keys. Also not keys which are only different in casing.
    /// * Also ensure you don't use spaces, dots or special characters in keys
    /// </remarks>
    IDictionary<string, object?> Attributes(RawConvertOptions options);
}