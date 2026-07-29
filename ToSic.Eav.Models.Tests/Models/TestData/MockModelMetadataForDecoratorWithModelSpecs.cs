namespace ToSic.Eav.Models.TestData;

[ModelSpecs(ContentType = nameof(MockModelMetadataForDecorator))]
internal record MockModelMetadataForDecoratorWithModelSpecsNameRight
    : MockModelMetadataForDecorator;


[ModelSpecs(ContentType = "WrongName")]
internal record MockModelMetadataForDecoratorWithModelSpecsNameWrong
    : MockModelMetadataForDecorator;


[ModelSpecs(ContentType = "*")]
internal record MockModelMetadataForDecoratorWithModelSpecsNameAsterisks
    : MockModelMetadataForDecorator;