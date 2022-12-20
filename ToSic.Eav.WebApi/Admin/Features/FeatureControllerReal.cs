﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public class FeatureControllerReal : ServiceBase, IFeatureController
    {
        public const string LogSuffix = "Feats";

        #region Constructor / DI

        public FeatureControllerReal(
            LazyInitLog<EavSystemLoader> systemLoaderLazy
            ) : base("Bck.Feats")
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
            
            return wrapLog.ReturnAsOk(_systemLoaderLazy.Value.UpdateFeatures(changes));
        }
    }
}
