using ToSic.Eav.Documentation;

namespace ToSic.Eav.Caching
{
    /// <summary>
    /// Decorate objects - usually AppStates - which can delegate the Expiry to more complex logic
    /// </summary>
    [PrivateApi("Internal stuff, could change at any time")]
    internal interface ICacheExpiringDelegated
    {
        ICacheExpiring CacheExpiryDelegate { get; }
    }
}
