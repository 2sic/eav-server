using ToSic.Eav.Data.ContentTypes;

namespace ToSic.Eav.Models.TestData;

[ModelSpecs(ContentType = nameof(MockMetadataModel))]
internal record MockMetadataModelWithSpecsNameRight
    : MockMetadataModel;


[ModelSpecs(ContentType = "WrongName")]
internal record MockMetadataModelWithSpecsNameWrong
    : MockMetadataModel;


[ModelSpecs(ContentType = "*")]
internal record MockMetadataModelWithSpecsNameAsterisks
    : MockMetadataModel;

[ContentType(
    Name = nameof(MockMetadataModel),
    Guid = "00000000-0000-0000-0000-000000000000",
    Description = ""
)]
internal record MockMetadataModelWithContentTypeSpecsName
    : MockMetadataModel;