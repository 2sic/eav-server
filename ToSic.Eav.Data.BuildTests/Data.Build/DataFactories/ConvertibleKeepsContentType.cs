using ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using static ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes.CodeTypeSpecsConstants;

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test to verify that the resulting Entity has a ContentType as specified.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class ConvertibleKeepsContentType(IDataFactory dataFactory)
{
    private void NameNotSetUsesDefault<TType>() where TType: IConvertibleToRawEntity, new()
    {
        var x = new TType();
        var y = dataFactory.CreateTac(x);
        NotNull(y);
        Equal(DataConstants.DataFactoryDefaultTypeName, y.Type.Name);
    }
    
    private void NameIsSet<T>(string expectedName) where T : IConvertibleToRawEntity, new()
    {
        var x = new T();
        var y = dataFactory.CreateTac(x);
        NotNull(y);
        Equal(expectedName, y.Type.Name);
    }
    
    [Fact]
    public void WithoutNewSpecsRawIsNotSet() =>
        NameNotSetUsesDefault<CodeTypeWithSpecsClassConvertibleNoSpecs>();

    [Fact]
    public void WithoutNewSpecsConvertibleIsNotSet() =>
        NameNotSetUsesDefault<CodeTypeWithSpecsClassConvertibleNoSpecsConverter>();

    [Fact]
    public void WithNewSpecsRawIsSet() =>
        NameIsSet<CodeTypeWithSpecsClassConvertibleWithSpecs>(CodeTypeSpecsConstants.SpecName);
    

    [Fact]
    public void WithNewSpecsConvertibleIsSet() =>
        NameIsSet<CodeTypeWithSpecsClassConvertibleWithSpecsConvertible>(CodeTypeSpecsConstants.SpecName);
    
}

public class CodeTypeWithSpecsClassConvertibleNoSpecs : CodeTypeSpecsYesClass, IRawEntity
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }
}


public class CodeTypeWithSpecsClassConvertibleNoSpecsConverter : CodeTypeSpecsYesClass, IGetRawConverter
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
public class CodeTypeWithSpecsClassConvertibleWithSpecs : CodeTypeSpecsYesClass, IRawEntity
{
    public DateTime Modified { get; }
    public IDictionary<string, object?> Values { get; }
}

[ContentTypeSpecs(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
public class CodeTypeWithSpecsClassConvertibleWithSpecsConvertible : CodeTypeSpecsYesClass, IGetRawConverter
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