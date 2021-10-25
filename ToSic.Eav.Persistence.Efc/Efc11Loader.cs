using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Types;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: HasLog<IRepositoryLoader>, IRepositoryLoader
    {
        #region constructor and private vars

        public Efc11Loader(EavDbContext dbContext,
            Lazy<IZoneCultureResolver> environmentLazy,
            IServiceProvider serviceProvider,
            IAppInitializedChecker initializedChecker,
            IAppStates appStates,
            LogHistory logHistory,
            GlobalTypes globalTypes
            ) : base("Db.Efc11")
        {
            ServiceProvider = serviceProvider;
            _dbContext = dbContext;
            _environmentLazy = environmentLazy;
            _initializedChecker = initializedChecker;
            _appStates = appStates;
            _logHistory = logHistory;
            _globalTypes = globalTypes;
        }

        public Efc11Loader UseExistingDb(EavDbContext dbContext)
        {
            _dbContext = dbContext;
            return this;
        }

        private IServiceProvider ServiceProvider { get; }
        private EavDbContext _dbContext;
        private readonly Lazy<IZoneCultureResolver> _environmentLazy;
        private readonly IAppInitializedChecker _initializedChecker;
        private readonly IAppStates _appStates;
        private readonly LogHistory _logHistory;
        private readonly GlobalTypes _globalTypes;

        #endregion
    }
}
