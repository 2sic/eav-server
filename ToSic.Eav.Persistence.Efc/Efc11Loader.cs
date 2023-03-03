using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader: ServiceBase, IRepositoryLoader
    {
        private readonly Generator<IAppRepositoryLoader> _appFileContentTypesLoader;
        private readonly Generator<IDataDeserializer> _dataDeserializer;

        #region constructor and private vars

        public Efc11Loader(
            EavDbContext dbContext,
            LazySvc<IZoneCultureResolver> environmentLazy,
            IAppInitializedChecker initializedChecker,
            IAppStates appStates,
            ILogStore logStore,
            LazySvc<IFeaturesInternal> featuresService,
            DataBuilder dataBuilder,
            Generator<IDataDeserializer> dataDeserializer,
            Generator<IAppRepositoryLoader> appFileContentTypesLoader) : base("Db.Efc11")
        {
            ConnectServices(
                _dbContext = dbContext,
                _environmentLazy = environmentLazy,
                _initializedChecker = initializedChecker,
                _appStates = appStates,
                _logStore = logStore,
                _featuresService = featuresService,
                _dataBuilder = dataBuilder,
                _dataDeserializer = dataDeserializer,
                _appFileContentTypesLoader = appFileContentTypesLoader
            );
        }

        public Efc11Loader UseExistingDb(EavDbContext dbContext)
        {
            _dbContext = dbContext;
            return this;
        }

        private EavDbContext _dbContext;
        private readonly LazySvc<IZoneCultureResolver> _environmentLazy;
        private readonly IAppInitializedChecker _initializedChecker;
        private readonly IAppStates _appStates;
        private readonly ILogStore _logStore;
        private readonly LazySvc<IFeaturesInternal> _featuresService;
        private readonly DataBuilder _dataBuilder;

        #endregion
    }
}
