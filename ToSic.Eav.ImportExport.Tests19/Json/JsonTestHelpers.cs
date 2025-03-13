using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization.Internal;

namespace ToSic.Eav.ImportExport.Tests19.Json;

public class JsonTestHelpers(EavDbContext dbContext, EfcAppLoader loader, Generator<JsonSerializer> jsonSerializerGenerator)
{
#if NETCOREAPP
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
#endif
    private EfcAppLoader Loader => field ??= loader.UseExistingDb(dbContext);

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