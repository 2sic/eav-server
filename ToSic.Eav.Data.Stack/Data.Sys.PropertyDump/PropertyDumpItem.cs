﻿namespace ToSic.Eav.Data.Sys.PropertyDump;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record PropertyDumpItem
{
    #region Constants and Static Helpers

    public const string Separator = ".";

    public static bool ShouldStop(string path)
        => path?.Length > 200;

    public static PropertyDumpItem DummyErrorShouldStop(string path)
    {
        var errPath = path + Separator + "ErrorTooDeep";
        return new()
        {
            Path = errPath,
            Property = new(result: "error", valueType: ValueTypesWithState.Virtual, path: new PropertyLookupPath().Add(errPath))
            {
                Name = "error",
            }
        };
    }

    #endregion

    /// <summary>
    /// The source of this item
    /// </summary>
    public string SourceName { get; set; } = "undefined";

    /// <summary>
    /// The priority of this source - for proper sorting / priorities
    /// </summary>
    public int SourcePriority { get; set; }

    /// <summary>
    /// Path to this property
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Result data of the property
    /// </summary>
    public PropReqResult? Property { get; set; }

    public List<PropertyDumpItem>? AllOptions { get; set; }
}