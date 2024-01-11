using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Assets;

/// <summary>
/// Size information for files
/// </summary>
/// <remarks>
/// * Added in v14.04 as class, changed to interface in v17
/// </remarks>
[PublicApi]
public interface ISizeInfo
{
    /// <summary>
    /// Size in bytes
    /// </summary>
    int Bytes { get; }

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
}