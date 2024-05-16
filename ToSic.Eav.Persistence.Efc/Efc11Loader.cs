using ToSic.Eav.Context;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.Efc;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class Efc11Loader(
    EavDbContext context,
    LazySvc<IZoneCultureResolver> environmentLazy,
    IAppInitializedChecker initializedChecker,
    IAppStates appStates,
    ILogStore logStore,
    LazySvc<IEavFeaturesService> featuresService,
    DataBuilder dataBuilder,
    Generator<IDataDeserializer> dataDeserializer,
    Generator<IAppContentTypesLoader> appFileContentTypesLoader,
    Generator<IAppStateBuilder> appStateBuilder)
    : ServiceBase("Db.Efc11",
        connect:
        [
            context, environmentLazy, initializedChecker, appStates, logStore, featuresService, dataBuilder,
            dataDeserializer, appFileContentTypesLoader, appStateBuilder
        ]), IRepositoryLoader
{

    public Efc11Loader UseExistingDb(EavDbContext dbContext)
    {
        context = dbContext;
        return this;
    }
}