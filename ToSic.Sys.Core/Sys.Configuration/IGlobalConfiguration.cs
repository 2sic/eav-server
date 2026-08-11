using System.Runtime.CompilerServices;

namespace ToSic.Sys.Configuration;

/// <summary>
/// Global configuration system.
/// </summary>
/// <remarks>
/// Should be setup as singleton.
/// May someday be replaced by ConfigurationManager or other standard .net system.
///
/// Note that accessing any properties should happen through extension methods, which themselves ensure that
/// default values and cleanup are done.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IGlobalConfiguration: ILogShouldNeverConnect
{
    /// <summary>
    /// Get a configuration with the same name as the calling method/property.
    /// </summary>
    /// <param name="key">The name of the configuration key, automatically set to the calling method/property name.</param>
    /// <returns>The value of the configuration key, or null if it doesn't exist.</returns>
    string? GetThis([CallerMemberName] string? key = default);

    /// <summary>
    /// Get a configuration with the same name as the calling method/property, or set it if it doesn't exist.
    /// </summary>
    /// <param name="generator">A function to generate the value if it doesn't exist.</param>
    /// <param name="key">The name of the configuration key, automatically set to the calling method/property name.</param>
    /// <returns>The value of the configuration key.</returns>
    string? GetThisOrSet(Func<string> generator, [CallerMemberName] string? key = default);

    /// <summary>
    /// Get a configuration with the same name as the calling method/property, or throw an error if it doesn't exist.
    /// </summary>
    /// <param name="key">The name of the configuration key, automatically set to the calling method/property name.</param>
    /// <returns>The value of the configuration key.</returns>
    string GetThisErrorOnNull([CallerMemberName] string? key = default);

    /// <summary>
    /// Set a configuration with the same name as the calling method/property.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="key">The name of the configuration key, automatically set to the calling method/property name.</param>
    /// <returns>The value of the configuration key.</returns>
    string? SetThis(string? value, [CallerMemberName] string? key = default);

}