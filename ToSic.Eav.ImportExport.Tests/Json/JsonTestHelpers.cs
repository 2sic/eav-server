using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Sys;

namespace ToSic.Eav.ImportExport.Tests.Json;

public class JsonTestHelpers(EavDbContext dbContext, EfcAppLoaderService loader, Generator<JsonSerializer> jsonSerializerGenerator)
{
#if NETCOREAPP
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
    private EfcAppLoaderService Loader => field ??= loader.UseExistingDb(dbContext);

    public JsonSerializer SerializerOfApp(int appId)
    {
        var app = Loader.AppStateReaderRawTac(appId);
        return jsonSerializerGenerator.New().SetApp(app);
    }

    internal string GetJsonOfEntity(int appId, int eId, JsonSerializer? ser = null)
    {
        var exBuilder = ser ?? SerializerOfApp(appId);
        var xmlEnt = exBuilder.Serialize(eId);
        return xmlEnt;
    }

    internal static string JsonOfContentType(JsonSerializer ser, IContentType type)
        => ser.Serialize(type, new()
        {
            // TODO: THESE settings probably don't affect the tests, should disable
            CtWithEntities = true,
            CtAttributeIncludeInheritedMetadata = true,
        });
}