using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.Build.DataFactories.MockData;

[ContentTypeSpecs(
    Name = nameof(MockRawWithNameTitle),
    Guid = "e38b4943-ceee-46a4-90fa-e89df35a3d5e",
    Description = "Mock Raw Entity with Name and Title"
)]
internal record MockRawWithNameTitle : MockRawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)]
    public string Name { get; init; }
}

/// <summary>
/// This is a RawEntity, but if things work correctly, it will not provide its own data, but instead the test-raw-entity in the constructor.
/// </summary>
/// <param name="_dataToProvideInConverter"></param>
// ReSharper disable once InconsistentNaming
[ContentTypeSpecs(
    Name = nameof(MockRawWithNameTitleProvidingConversion), // this must also have specs, otherwise the IsTitle won't be retrieved
    Guid = "5154061f-c028-4a5c-bd25-4e618c597ee1",
    Description = "Mock Raw Entity with Name and Title"
)]
internal record MockRawWithNameTitleProvidingConversion(IRawEntity _dataToProvideInConverter) : MockRawWithNameTitle, IRawEntityConvertible
{
    // Title must be specified in this record, because it's the one used for
    // Schema lookup, not the final RawEntity returned by the converter.
    // So we need to make sure it has the correct attribute.
    [ContentTypeAttributeSpecs(IsTitle = true)]
    public string Name { get; init; }

    public IRawEntityConverter GetConverter() =>
        new RawEntityConverterFactory<MockRawWithNameTitleProvidingConversion>((_, _) =>
            _dataToProvideInConverter
        );
}