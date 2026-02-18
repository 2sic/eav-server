using System.Runtime.CompilerServices;

namespace ToSic.Eav.DataSource;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal static class DataSourceConfigurationExtensionsObsolete
{
    /// <summary>
    /// Set a configuration value for a specific property.
    /// This is BAD practice, do not use this.
    /// This is necessary for older DataSources which hat configuration setters.
    /// We will not support that anymore, do not use.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="name">The configuration key. Do not set this; it's auto-added by the compiler.</param>
    /// <param name="config"></param>
    /// <remarks>Added in v15.04</remarks>
#pragma warning disable CS0618
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    internal static void SetThisObsolete<T>(this IDataSourceConfiguration config, T value, [CallerMemberName] string? name = default)
        => ((DataSourceConfiguration)config).SetThisObsolete(value, name);
#pragma warning restore CS0618
}