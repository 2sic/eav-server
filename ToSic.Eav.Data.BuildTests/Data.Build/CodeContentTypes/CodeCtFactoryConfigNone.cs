using ToSic.Eav.Data.Build.Sys;

namespace ToSic.Eav.Data.Build.CodeContentTypes;
// ReSharper disable UnusedMember.Global

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Class** having no configuration.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfigNoneClass(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecs>(ctDefManager);

/// <summary>
/// Test conversion of a type to a ContentType Definition - with a **Record** having no configuration.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeCtFactoryConfigNoneRecord(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecsRecord>(ctDefManager);

// ReSharper restore UnusedMember.Global

/// <summary>
/// Shared (abstract) tests for classes or records which are not configured (no attributes)
/// </summary>
public abstract class CodeCtFactoryConfigNone<TRawEntity>(CodeContentTypesManager ctDefManager)
{
    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, ctDefManager.CreateTac<TRawEntity>().Attributes.Count());
   
    
    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(ctDefManager.GetVirtualAttribDecorator(typeof(TRawEntity)));
    
    
    [Theory]
    [InlineData(nameof(CodeTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => ctDefManager.CreateTac<TRawEntity>().AssertAttribute(name, type);
    
}
