using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;
using ToSic.Eav.Serialization.Sys;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class JsonTestHelpers(EavDbContext dbContext, EfcAppLoaderService loader, Generator<IJsonDataWriter> jsonWriteGen, Generator<IJsonDataReader, IJsonDataReader.Options> jsonReadGen)
{
#if NETCOREAPP
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
    private EfcAppLoaderService Loader => field ??= loader.UseExistingDb(dbContext);

    public IJsonDataWriter JsonWriterOfApp(int appId) => jsonWriteGen.New();

    public IJsonDataReader JsonReaderOfApp(int appId)
    {
        var appReader = Loader.AppStateReaderRawTac(appId);
        return jsonReadGen.New(new(appReader));
    }
    public (IAppReader Reader, IJsonDataWriter Serializer) ReaderAndSerializerOfApp(int appId)
    {
        var appReader = Loader.AppStateReaderRawTac(appId);
        return (appReader, jsonWriteGen.New());
    }

    internal string GetJsonOfEntity(int appId, int eId, IJsonDataWriter? ser = null)
    {
        var appReader = Loader.AppStateReaderRawTac(appId);
        var exBuilder = ser ?? JsonWriterOfApp(appId);
        var xmlEnt = exBuilder.Serialize(appReader.List.FindRepoId(eId) ?? throw new NullReferenceException($"Entity {eId} not found, can't serialize"));
        return xmlEnt;
    }

    internal static string JsonOfContentType(IJsonDataWriter ser, IContentType type)
        => ser.Serialize(type, new()
        {
            // TODO: THESE settings probably don't affect the tests, should disable
            CtWithEntities = true,
            CtAttributeIncludeInheritedMetadata = true,
        });
}