using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.DataTests;

[Startup(typeof(StartupTestsEavDataBuild))]
public class InputTypeInfoVerifyFields(DataFactoryGenerator dataFactory)
{
    [Fact]
    public void ObjectFieldsEnableUnknownValueConversion()
    {
        var raw = new InputTypeInfoRaw
        {
            Type = "test",
            Label = null,
            Description = null,
            DisableI18n = false,
            // This one is value-type "Object"
            UiAssets = new Dictionary<string, string> { ["main"] = "main.js" },
            UseAdam = false,
            IsObsolete = false,
            ObsoleteMessage = null,
            IsRecommended = false,
            IsDefault = false,
            Source = null,
            // This one is value-type "Object"
            ConfigTypes = ["TestConfig"],
        };

        var entity = dataFactory.New(new()).Create(raw);

        Equal(ValueTypes.Object, entity.Attributes[nameof(InputTypeInfoRaw.UiAssets)].Type);
        Equal(ValueTypes.Object, entity.Attributes[nameof(InputTypeInfoRaw.ConfigTypes)].Type);
    }

}