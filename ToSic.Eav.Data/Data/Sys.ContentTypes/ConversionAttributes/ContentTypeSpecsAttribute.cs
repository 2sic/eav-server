﻿namespace ToSic.Eav.Data.Sys.ContentTypes;

/// <summary>
/// Content type information for use when converting a POCO to a Content Type.
/// </summary>
[PrivateApi("WIP")]
[ShowApiWhenReleased(ShowApiMode.Never)]
[AttributeUsage(AttributeTargets.Class)]
public class ContentTypeSpecsAttribute: Attribute
{
    /// <summary>
    /// Content Type Guid, as a string, because GUIDs are not supported in attributes
    /// </summary>
    public required string Guid { get; set; }

    /// <summary>
    /// Content type name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Content type description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Content Type Scope - if blank, will default to "Default"
    /// </summary>
    public string Scope { get; set; } = "";
}