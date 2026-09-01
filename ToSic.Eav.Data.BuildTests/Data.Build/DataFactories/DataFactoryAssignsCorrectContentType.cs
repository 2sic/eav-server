using ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes;
using ToSic.Eav.Data.Build.DataFactories.MockData;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using static ToSic.Eav.Data.Build.ContentTypesFromCode.SpecsYes.CodeTypeSpecsConstants;
// ReSharper disable UnassignedGetOnlyAutoProperty
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test to verify that the resulting Entity has a ContentType as specified.
/// </summary>
[Startup(typeof(StartupTestsEavDataBuild))]
public class DataFactoryAssignsCorrectContentType(IDataFactory dataFactory)
{
    #region Name Checks for resulting Content Type

    private void NameNotSetUsesDefault<T>() where T: IRawData, new() =>
        NameIsSet<T>(DataConstants.DataFactoryDefaultTypeName);

    private void NameIsSet<T>(string expectedName) where T : IRawData, new()
    {
        var x = new T();
        var y = dataFactory.CreateTac(x);
        NotNull(y);
        Equal(expectedName, y.Type.Name);
    }

    #endregion

    #region Raw Without Specs

    [Fact]
    public void WithoutNewSpecsRawIsNotSet() =>
        NameNotSetUsesDefault<SourceRawSpecsNone>();

    private class SourceRawSpecsNone : CodeTypeSpecsYesClass, IRawEntity
    {
        public DateTime Modified { get; }
        public IDictionary<string, object?> Values { get; }
    }

    #endregion


    #region Source with Converter - without specs

    [Fact]
    public void WithoutNewSpecsConvertibleIsNotSet() =>
        NameNotSetUsesDefault<SourceConvertibleSpecsNone>();

    private class SourceConvertibleSpecsNone : CodeTypeSpecsYesClass, IRawEntityConvertible
    {
        public DateTime Modified { get; }
        public IDictionary<string, object?> Values { get; }

        public IRawEntityConverter GetConverter() =>
            new RawEntityConverterFactory<SourceConvertibleSpecsNone>((_, _) =>
                new MockRawEntity { Id = 0, }
            );
    }

    #endregion


    #region Source Raw With Specs

    [Fact]
    public void WithNewSpecsRawIsSet() =>
        NameIsSet<SourceRawSpecsYes>(SpecName);

    [ContentType(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
    private class SourceRawSpecsYes : CodeTypeSpecsYesClass, IRawEntity
    {
        public DateTime Modified { get; }
        public IDictionary<string, object?> Values { get; }
    }

    #endregion


    #region Source Convertible With Specs


    [Fact]
    public void WithNewSpecsConvertibleIsSet() =>
        NameIsSet<RawConvertibleWithSpecs>(SpecName);

    [ContentType(Name = SpecName, Guid = SpecGuid, Scope = SpecScope, Description = SpecDescription)]
    private class RawConvertibleWithSpecs : CodeTypeSpecsYesClass, IRawEntityConvertible
    {
        public DateTime Modified { get; }
        public IDictionary<string, object?> Values { get; }
        public IRawEntityConverter GetConverter() =>
            new RawEntityConverterFactory<RawConvertibleWithSpecs>((_, _) =>
                new MockRawEntity { Id = 0, }
            );
    }
    
    #endregion

}
