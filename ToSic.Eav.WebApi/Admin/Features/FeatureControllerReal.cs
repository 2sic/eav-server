using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.DI;
using ToSic.Eav.Logging;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public class FeatureControllerReal : WebApiBackendBase<FeatureControllerReal>, IFeatureController
    {
        public const string LogSuffix = "Feats";

        #region Constructor / DI

        public FeatureControllerReal(
            IServiceProvider serviceProvider,
            LazyInitLog<EavSystemLoader> systemLoaderLazy
            ) : base(serviceProvider, "Bck.Feats")
        {
            _systemLoaderLazy = systemLoaderLazy.SetLog(Log);
        }


        /// <summary>
        /// Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
        /// </summary>
        private readonly LazyInitLog<EavSystemLoader> _systemLoaderLazy;

        #endregion

        public bool SaveNew(List<FeatureManagementChange> changes)
        {
            var wrapLog = Log.Fn<bool>();

            // validity check 
            if (changes == null || changes.Count == 0) 
                return wrapLog.ReturnFalse("no features changes");
            
            return wrapLog.ReturnAsOk(_systemLoaderLazy.Ready.UpdateFeatures(changes));
        }
    }
}
