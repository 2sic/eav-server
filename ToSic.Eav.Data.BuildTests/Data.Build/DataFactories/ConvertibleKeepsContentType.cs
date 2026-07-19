using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test to verify that the resulting Entity has a ContentType as specified.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ConvertibleKeepsContentType(IDataFactory dataFactory)
{
    // TODO: 2dm continue here 2026-07-19
    [Fact]
    public void WithoutNewSpecsRawIsNotSet()
    {
        var x = new CodeTypeWithSpecsClassConvertibleNoSpecs();
        var y = dataFactory.CreateTac(x);
        NotNull(y);
        Equal(DataConstants.DataFactoryDefaultTypeName, y.Type.Name);
        
    }
    
    [Fact]
    public void WithoutNewSpecsConvertibleIsNotSet()
    {
        var x = new CodeTypeWithSpecsClassConvertibleNoSpecsConverter();
        var y = dataFactory.CreateTac(x);
        NotNull(y);
        Equal(DataConstants.DataFactoryDefaultTypeName, y.Type.Name);
    }

    [Fact]
    public void WithNewSpecsRawIsSet()
    {
        var x = new CodeTypeWithSpecsClassConvertibleWithSpecs();
        var y = dataFactory.CreateTac(x);
        Equal(CodeTypeWithSpecsEmpty.SpecName, y.Type.Name);
    }
    [Fact]
    public void WithNewSpecsConvertibleIsSet()
    {
        var x = new CodeTypeWithSpecsClassConvertibleWithSpecsConvertible();
        var y = dataFactory.CreateTac(x);
        Equal(CodeTypeWithSpecsEmpty.SpecName, y.Type.Name);
    }
    
}

public class CodeTypeWithSpecsClassConvertibleNoSpecs : CodeTypeWithSpecsClass, IRawEntity
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }
}


public class CodeTypeWithSpecsClassConvertibleNoSpecsConverter : CodeTypeWithSpecsClass, IGetRawConverter
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }

    public IRawEntityConverter GetConverter() =>
        new ConvertToRawWithFactory<CodeTypeWithSpecsClassConvertibleNoSpecsConverter>((_, _) =>
            new MockRawEntityRecord
            {
                Id = 0,
            }
        );
}

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeWithSpecsClassConvertibleWithSpecs : CodeTypeWithSpecsClass, IRawEntity
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }
}

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeWithSpecsClassConvertibleWithSpecsConvertible : CodeTypeWithSpecsClass, IGetRawConverter
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }
    public IRawEntityConverter GetConverter() =>
        new ConvertToRawWithFactory<CodeTypeWithSpecsClassConvertibleWithSpecsConvertible>((_, _) =>
            new MockRawEntityRecord
            {
                Id = 0,
            }
        );

}