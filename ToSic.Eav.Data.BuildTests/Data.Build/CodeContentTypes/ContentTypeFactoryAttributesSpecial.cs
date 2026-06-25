using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

/// <summary>
/// Tests for configured classes (with attributes)
/// </summary>
/// <param name="ctDefFactory"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ContentTypeFactoryAttributesSpecial(CodeContentTypesManager ctDefFactory)
{

    [Fact]
    public void Attributes_InternalFields()
    {
        var x = ctDefFactory.CreateTac<CodeTypeInternalFields>();
        Single(x.Attributes);
    }

}
