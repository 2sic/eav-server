namespace ToSic.Eav.Models.TestData;

/// <summary>
/// This will be used to verify that the name will not work if it's wrong.
/// Resulting in filtering for data which doesn't exist, and thus returning null.
/// </summary>
internal record MockModelMetadataForDecoratorWrongName
    : MockModelMetadataForDecorator;