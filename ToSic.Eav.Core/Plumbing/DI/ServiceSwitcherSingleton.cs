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

        public ServiceSwitcherSingleton(
            LogHistory logHistory,
            LazyInitLog<ServiceSwitcher<T>> serviceSwitcher
        ) : base($"{LogNames.Eav}.SrvSwS")
        {
            _logHistory = logHistory;
            _serviceSwitcher = serviceSwitcher.SetLog(Log);
        }
        private readonly LogHistory _logHistory;
        private readonly LazyInitLog<ServiceSwitcher<T>> _serviceSwitcher;

        public T Value => GetSingletonSwitchableService();

        private T GetSingletonSwitchableService()
        {
            // Already loaded
            if (_preferredService != null) return _preferredService;

            _logHistory.Add(LogNames.LogHistoryGlobalAndStartUp, Log);
            var call = Log.Call2<T>(message: "re-check singleton service");
            _preferredService = _serviceSwitcher.Ready.Value;
            return call.Return(_preferredService, $"found {_preferredService.NameId}");
        }

        /// <summary>
        /// Note: This must be static, as the service itself is transient, not singleton!
        /// </summary>
        private static T _preferredService;

        public bool IsValueCreated => _preferredService != null;

        public T ByNameId(string nameId) => _serviceSwitcher.Ready.ByNameId(nameId);

        protected void Reset() => _preferredService = default;
    }
}
