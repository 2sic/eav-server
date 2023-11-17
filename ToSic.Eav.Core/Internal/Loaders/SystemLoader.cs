using System;
using System.Collections.Generic;
using ToSic.Eav.StartUp;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Loaders
{
    /// <summary>
    /// WIP - the main loader which will run pre-loaders first, then the main loader
    /// </summary>
    public class SystemLoader : ServiceBase
    {
        public SystemLoader(
            ILogStore logStore,
            IEnumerable<IStartUpRegistrations> registrations,
            LazySvc<EavSystemLoader> systemLoaderLazy // This must be lazy, as some dependencies of it could change till it's needed
        ) : base($"{EavLogs.Eav}SysLdr")
        {
            logStore.Add(Lib.Logging.LogNames.LogStoreStartUp, Log);
            Log.A("EAV System Loader");
            ConnectServices(
                _registrations = registrations,
                _systemLoaderLazy = systemLoaderLazy
            );
        }

        private readonly IEnumerable<IStartUpRegistrations> _registrations;
        private readonly LazySvc<EavSystemLoader> _systemLoaderLazy;

        /// <summary>
        /// This is just for public access, don't use in this file
        /// </summary>
        public EavSystemLoader EavSystemLoader => _systemLoaderLazy.IsValueCreated
            ? _systemLoaderLazy.Value
            : throw new Exception("Can't access this property unless StartUp has run first");

        public void StartUp() => Log.Do(l =>
        {
            DoRegistrations();
            l.A("Will now run StartUp on EAv SystemLoader - logs are tracked separately");
            _systemLoaderLazy.Value.StartUp();
        });

        private void DoRegistrations() => Log.Do(() =>
        {
            foreach (var registration in _registrations)
                DoRegistration(registration);
        });

        private void DoRegistration(IStartUpRegistrations registration) => Log.Do(registration.NameId, l =>
        {
            try
            {
                // TODO: to remove this init, we need to implement something in the ConnectService #dropLogInit
                // which can handle DI-IEnumerables. To dev this we would need unit tests
                registration.LinkLog(Log);
                registration.Register();
            }
            catch (Exception ex)
            {
                l.A($"Error on registration of {registration.NameId}");
                l.Ex(ex);
            }
        });
}
}
