using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes.SystemProperties;

/// <summary>
/// Tests for configured classes (with attributes)
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactorySystemProperties(CodeContentTypesManager ctDefManager)
{

    [Fact]
    public void Attributes_OnlyOneInternalFields()
    {
        var x = ctDefManager.CreateTac<CodeTypeWithSystemProperties>();
        Single(x.Attributes);
    }

}
