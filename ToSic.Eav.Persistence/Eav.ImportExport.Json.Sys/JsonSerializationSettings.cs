﻿namespace ToSic.Eav.ImportExport.Json.Sys;

/// <summary>
/// Controls how serialization should happen, as different scenarios require different parts to be included or not.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonSerializationSettings
{
    /// <summary>
    /// Include definitions of ContentTypes which are just inherited / virtual,
    /// as they are based on a parent ContentType
    /// </summary>
    public bool CtIncludeInherited { get; set; } = false;

    /// <summary>
    /// Include Metadata of Attributes which only inherit their Metadata.
    /// </summary>
    public bool CtAttributeIncludeInheritedMetadata { get; set; } = true;

    public bool CtWithEntities { get; set; } = true;
}