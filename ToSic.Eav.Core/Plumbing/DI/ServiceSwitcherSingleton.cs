using ToSic.Eav.Logging;

namespace ToSic.Eav.Plumbing.DI
{
    /// <summary>
    /// Similar to the <see cref="ServiceSwitcher{T}"/> but special.
    /// It will behave as singleton, but will resolved Transient! this is important.
    /// Reason is that this way we don't keep a list of all possible services active in memory, just the one that's been selected.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceSwitcherSingleton<T>: HasLog, ILazyLike<T> where T : ISwitchableService
    {
        public ServiceSwitcherSingleton(LazyInitLog<ServiceSwitcher<T>> serviceSwitcher) : base(
            $"{LogNames.Eav}.SrvSwS")
            => _serviceSwitcher = serviceSwitcher.SetLog(Log);
        private readonly LazyInitLog<ServiceSwitcher<T>> _serviceSwitcher;

        public T Value => _preferredService != null ? _preferredService : _preferredService = _serviceSwitcher.Ready.Value;

        /// <summary>
        /// Note: This must be static, as the service itself is transient, not singleton!
        /// </summary>
        private static T _preferredService;

        public bool IsValueCreated => _preferredService != null;

        public T ByNameId(string nameId) => _serviceSwitcher.Ready.ByNameId(nameId);
    }
}
