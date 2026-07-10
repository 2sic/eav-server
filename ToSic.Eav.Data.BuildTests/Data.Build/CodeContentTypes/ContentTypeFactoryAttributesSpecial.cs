using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Tests for configured classes (with attributes)
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryAttributesSpecial(CodeContentTypesManager ctDefManager)
{

    [Fact]
    public void Attributes_OnlyOneInternalFields()
    {
        var x = ctDefManager.CreateTac<CodeTypeInternalFields>();
        Single(x.Attributes);
    }

}
