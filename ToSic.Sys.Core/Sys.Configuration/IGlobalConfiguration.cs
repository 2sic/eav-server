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
public interface IGlobalConfiguration: ILogShouldNeverConnect
{
    string? GetThis([CallerMemberName] string? key = default);

    string? GetThisOrSet(Func<string> generator, [CallerMemberName] string? key = default);

    string GetThisErrorOnNull([CallerMemberName] string? key = default);

    string? SetThis(string? value, [CallerMemberName] string? key = default);

}