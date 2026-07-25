using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.Build.DataFactories.MockData;

[ContentTypeSpecs(
    Name = "MockRawWithNameTitle",
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
internal record MockRawWithNameTitleProvidingConversion(IRawEntity _dataToProvideInConverter) : MockRawWithNameTitle, IRawEntityConvertible
{
    public IRawEntityConverter GetConverter() =>
        new RawEntityConverterFactory<MockRawWithNameTitleProvidingConversion>((_, _) =>
            _dataToProvideInConverter
        );
}