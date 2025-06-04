using System.Collections.Immutable;

namespace ToSic.Eav.Metadata.Targets;

/// <summary>
/// This loads the TargetTypes from the DB or wherever they are stored.
/// </summary>
[PrivateApi("was internal till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ITargetTypesLoader
{
    ImmutableDictionary<int, string> GetTargetTypes();
}