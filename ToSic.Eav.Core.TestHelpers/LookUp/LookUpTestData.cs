using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp.Sources;
using ToSic.Lib.LookUp;
using ToSic.Lib.LookUp.Engines;

namespace ToSic.Eav.LookUp;

public class LookUpTestData(DataBuilder builder)
{
    public static LookUpEngine EmptyLookupEngine(List<ILookUp> sources = null)
        => new(null, sources: sources);

    public LookUpEngine AppSetAndRes(int appId = LookUpTestConstants.AppIdUnknown, List<ILookUp> sources = null)
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
        var ent = builder.CreateEntityTac(appId: appId, contentType: builder.ContentType.Transient(name), values: values, titleField: values.FirstOrDefault().Key);
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