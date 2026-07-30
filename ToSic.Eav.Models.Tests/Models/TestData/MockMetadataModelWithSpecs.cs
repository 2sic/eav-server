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