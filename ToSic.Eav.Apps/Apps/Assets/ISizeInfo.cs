namespace ToSic.Eav.Apps.Assets;

/// <summary>
/// Size information for files
/// </summary>
/// <remarks>
/// * Added in v14.04 as class, changed to interface in v17
/// * Updated to use long in v21.06, as some data can be larger than 2GB, which is the limit of int.
/// * Added ToString() in v21.06 to show the best size and unit in a human-readable format, e.g. "1.23 MB"
/// </remarks>
[PublicApi]
public interface ISizeInfo
{
    /// <summary>
    /// Size in bytes.
    /// </summary>
    /// <remarks>Type changed from `int` to `long` in v21.06</remarks>
    long Bytes { get; }

    /// <summary>
    /// Size in KB
    /// </summary>
    /// <returns></returns>
    decimal Kb { get; }

    /// <summary>
    /// Size in MB
    /// </summary>
    /// <returns></returns>
    decimal Mb { get; }

    /// <summary>
    /// Size in GB
    /// </summary>
    /// <returns></returns>
    decimal Gb { get; }

    /// <summary>
    /// Best size based on the number. Will be in KB, MB or GB.
    /// The unit is found on BestUnit
    /// </summary>
    /// <returns></returns>
    decimal BestSize { get; }

    /// <summary>
    /// Best unit to use based on the effective size. 
    /// </summary>
    /// <returns></returns>
    string BestUnit { get; }

    /// <summary>
    /// Show the best size and best unit in a human-readable format, e.g. "1.23 MB"
    /// </summary>
    /// <remarks>Added v21.06</remarks>
    string ToString();
}