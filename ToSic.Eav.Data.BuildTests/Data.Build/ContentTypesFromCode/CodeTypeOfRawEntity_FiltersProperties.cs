using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Raw.Sys;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ToSic.Eav.Data.Build.ContentTypesFromCode;

/// <summary>
/// Verify that additional properties such as "ID" or "Metadata" - or especially "Values" don't end up in the content type definition, because they are not part of the content type definition, but part of the raw entity.
/// </summary>
/// <param name="ctDefManager"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class CodeTypeOfRawEntity_FiltersProperties(ContentTypesFromCodeManager ctDefManager)
{
    [Fact]
    public void CodeTypeOfRawEntityIgnoresExtraProps()
    {
        var type = ctDefManager.CreateTac<RawEntity>();
        //Equal(0, type.Attributes.Count());
        Empty(type.Attributes);
    }

    [Theory]
    [InlineData(typeof(RawEntity_NewValuesString))]
    [InlineData(typeof(RawEntity_NewValuesDicString))]
    [InlineData(typeof(RawEntity_NewValuesDicObject))]
    public void CodeTypeOfRawEntity_ValuesOverriden_HasValuesProp(Type sourceType)
    {
        var type = ctDefManager.CreateTac(sourceType);
        //Equal(1, type.Attributes.Count());
        Single(type.Attributes);
        Equal("Values", type.Attributes.First().Name);
    }


    // ReSharper disable InconsistentNaming

    private protected record RawEntity_NewValuesString : RawEntity
    {
        public new string Values { get; init; }
    }

    private protected record RawEntity_NewValuesDicString : RawEntity
    {
        public new Dictionary<string, string> Values { get; init; }
    }

    private protected record RawEntity_NewValuesDicObject : RawEntity
    {
        // Note: this is very similar, but a Dictionary instead of IDictionary, so it's treated as a new property
        public new Dictionary<string, object> Values { get; init; }
    }

    // ReSharper restore InconsistentNaming

}