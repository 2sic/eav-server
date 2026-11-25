using ToSic.Eav.Data.Values.Sys;

namespace ToSic.Eav.Data.Values.Unique;

/// <summary>
/// Test accessor methods for <see cref="IsUniqueValidator"/>.
/// Provides logic-free pass-through methods to keep production API usage analysis clean.
/// </summary>
internal static class IsUniqueValidatorTestAccessors
{
    /// <summary>
    /// Test accessor for <see cref="IsUniqueValidator.UniqueValuesOnly"/>.
    /// </summary>
    public static Exception? UniqueValuesOnlyTac(this IsUniqueValidator validator, IReadOnlyCollection<IEntity> pendingEntities)
        => validator.UniqueValuesOnly(pendingEntities);
}
