using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build.CodeContentTypes;

[Startup(typeof(StartupTestsEavDataBuild))]
// ReSharper disable once UnusedMember.Global
public class CodeCtFactoryConfigNoneClass(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecs>(ctDefManager);

[Startup(typeof(StartupTestsEavDataBuild))]
// ReSharper disable once UnusedMember.Global
public class CodeCtFactoryConfigNoneRecord(CodeContentTypesManager ctDefManager)
    : CodeCtFactoryConfigNone<CodeTypeNoSpecsRecord>(ctDefManager);

/// <summary>
/// Tests for classes or records which are not configured (no attributes)
/// </summary>
public abstract class CodeCtFactoryConfigNone<TRawEntity>(CodeContentTypesManager ctDefManager)
{
    private void AssertAttribute(IContentType ct, string name, ValueTypes type, bool isTitle = false, string? description = default)
    {
        var attribute = ct.Attributes.FirstOrDefault(a => a.Name == name);
        NotNull(attribute); //, $"{name} null check");
        Equal(name, attribute.Name); //, $"{name} Name check");
        Equal(type, attribute.Type); //, $"{name} type check");
        Equal(isTitle, attribute.IsTitle); //, $"{name} IsTitle check");
        if (description != default)
            Equal(description, attribute.Metadata.Get<string>(AttributeMetadataConstants.DescriptionField)); //, $"{name} Description check");
    }

    private ContentTypeVirtualAttributes GetVAttribDecorator(Type t)
        => ctDefManager.CreateTac(t).GetDecorator<ContentTypeVirtualAttributes>()!;

    private T GetPropNoSpecs<T>(Func<IContentType, T> getFunc)
        => getFunc(ctDefManager.CreateTac<TRawEntity>());
    
    [Fact]
    public void Attributes_NoSpec_Count()
        => Equal(4, GetPropNoSpecs(x => x.Attributes.Count()));
   
    
    [Fact]
    public void Attributes_NoSpec_NoVDecorator()
        => Null(GetVAttribDecorator(typeof(TRawEntity)));
    
    
    [Theory]
    [InlineData(nameof(CodeTypeNoSpecs.Name), ValueTypes.String)]
    [InlineData(nameof(CodeTypeNoSpecs.Age), ValueTypes.Number)]
    [InlineData(nameof(CodeTypeNoSpecs.BirthDate), ValueTypes.DateTime)]
    [InlineData(nameof(CodeTypeNoSpecs.IsAlive), ValueTypes.Boolean)]
    public void AssertAttributeNoSpec(string name, ValueTypes type)
        => AssertAttribute(ctDefManager.CreateTac<TRawEntity>(), name, type);
    
}
