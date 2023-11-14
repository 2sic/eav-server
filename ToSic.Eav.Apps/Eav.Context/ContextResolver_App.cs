﻿using System;
using ToSic.Eav.Apps;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context
{
    partial class ContextResolver
    {
        /// <summary>
        /// This is set whenever an App Context is retrieved.
        /// </summary>
        protected IContextOfApp LatestAppContext;

        public IContextOfApp SetApp(IAppIdentity appIdentity)
        {
            var appCtx = _contextOfApp.New();
            appCtx.ResetApp(appIdentity);
            LatestAppContext = appCtx;
            return appCtx;
        }

        public IContextOfApp App()
            => LatestAppContext ?? throw new Exception($"To call {nameof(App)} you must first call {nameof(SetApp)}");

    }
}
