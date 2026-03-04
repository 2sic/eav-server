using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.LookUp.TestHelpers;

namespace ToSic.Eav.LookUp;


public class LookUpTestData(DataAssembler dataAssembler, ContentTypeAssembler typeAssembler)
{
    private static LookUpEngine EmptyLookupEngine(List<ILookUp>? sources = null)
        => new(null, sources: sources);

    public LookUpEngine AppSetAndRes(int appId = LookUpTestConstants.AppIdUnknown, List<ILookUp>? sources = null)
    {
        sources ??= [];
        var vc = EmptyLookupEngine(sources: sources.Concat(new List<ILookUp>
            {
                AppSettings(appId),
                AppResources(appId)
            }).ToList()
        );
        return vc;
    }

    public LookUpInEntity BuildLookUpEntity(string name, Dictionary<string, object> values, int appId = LookUpTestConstants.AppIdUnknown)
    {
        var ent = dataAssembler.CreateEntityTac(appId: appId, contentType: typeAssembler.Type.Transient(name), values: values, titleField: values.FirstOrDefault().Key);
        return new(name, ent, null);
    }

    private LookUpInEntity AppSettings(int appId) =>
        BuildLookUpEntity(LookUpTestConstants.KeyAppSettings, new()
        {
            { AttributeNames.TitleNiceName, "App Settings" },
            { "DefaultCategoryName", LookUpTestConstants.DefaultCategory },
            { "MaxPictures", LookUpTestConstants.MaxPictures },
            { "PicsPerRow", "3" }
        }, appId);

    private LookUpInEntity AppResources(int appId) =>
        BuildLookUpEntity(LookUpTestConstants.KeyAppResources, new()
        {
            { AttributeNames.TitleNiceName, "Resources" },
            { "Greeting", "Hello there!" },
            { "Introduction", "Welcome to this" }
        }, appId);
}