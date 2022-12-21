using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: ServiceBase, IRepositoryLoader
    {
        #region constructor and private vars

        public Efc11Loader(
            EavDbContext dbContext,
            LazyInit<IZoneCultureResolver> environmentLazy,
            IServiceProvider serviceProvider,
            IAppInitializedChecker initializedChecker,
            IAppStates appStates,
            ILogStore logStore,
            LazyInit<IFeaturesInternal> featuresService,
            MultiBuilder multiBuilder
            ) : base("Db.Efc11")
        {
            ConnectServices(
                ServiceProvider = serviceProvider,
                _dbContext = dbContext,
                _environmentLazy = environmentLazy,
                _initializedChecker = initializedChecker,
                _appStates = appStates,
                _logStore = logStore,
                _featuresService = featuresService,
                _multiBuilder = multiBuilder
            );
        }

        public Efc11Loader UseExistingDb(EavDbContext dbContext)
        {
            _dbContext = dbContext;
            return this;
        }

        private IServiceProvider ServiceProvider { get; }
        private EavDbContext _dbContext;
        private readonly LazyInit<IZoneCultureResolver> _environmentLazy;
        private readonly IAppInitializedChecker _initializedChecker;
        private readonly IAppStates _appStates;
        private readonly ILogStore _logStore;
        private readonly LazyInit<IFeaturesInternal> _featuresService;
        private readonly MultiBuilder _multiBuilder;

        #endregion
    }
}
