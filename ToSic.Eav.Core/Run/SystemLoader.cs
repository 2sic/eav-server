using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// WIP - the main loader which will run pre-loaders first, then the main loader
    /// </summary>
    public class SystemLoader: HasLog
    {
        public SystemLoader(
            LogHistory logHistory,
            IEnumerable<IStartUpRegistrations> registrations,
            Lazy<EavSystemLoader> systemLoaderLazy // This must be lazy, as some dependencies of it could change till it's needed
        ) : base($"{LogNames.Eav}SysLdr", initialMessage: "EAV System Loader")
        {
            logHistory.Add(LogNames.LogHistoryGlobalAndStartUp, Log);
            _registrations = registrations;
            _systemLoaderLazy = systemLoaderLazy;
        }
        private readonly IEnumerable<IStartUpRegistrations> _registrations;
        private readonly Lazy<EavSystemLoader> _systemLoaderLazy;

        /// <summary>
        /// This is just for public access, don't use in this file
        /// </summary>
        public EavSystemLoader EavSystemLoader => _systemLoaderLazy.IsValueCreated
            ? _systemLoaderLazy.Value
            : throw new Exception("Can't access this property unless StartUp has run first");

        public void StartUp()
        {
            var call = Log.Fn();
            DoRegistrations();
            Log.A("Will now run StartUp on EAv SystemLoader - logs are tracked separately");
            _systemLoaderLazy.Value.StartUp();
            call.Done();
        }

        private void DoRegistrations()
        {
            var call = Log.Fn();
            foreach (var registration in _registrations) 
                DoRegistration(registration);
            call.Done();
        }

        private void DoRegistration(IStartUpRegistrations registration)
        {
            var callReg = Log.Fn(registration.NameId);
            try
            {
                registration.Init(Log);
                registration.Register();
            }
            catch (Exception ex)
            {
                Log.A($"Error on registration of {registration.NameId}");
                Log.Ex(ex);
            }
            callReg.Done();
        }
    }
}
