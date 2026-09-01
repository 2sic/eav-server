namespace ToSic.Eav.Models.TestData;

/// <summary>
/// Make sure the test data generators do what we expect.
/// </summary>
/// <param name="generator"></param>
// ReSharper disable once InconsistentNaming
public class MockDataGenerator_Verify(MockDataGenerator generator)
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void WithSameMetadataManyTimes(int amount)
    {
        var entity = generator.CreateEntityWithMetadata(amount);
        Equal(amount, entity.Metadata.Count());
    }


    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(5, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 3)]
    [InlineData(2, 4)]
    public void WithMixedMetadataManyTimes(int amountMdFor, int amountOther)
    {
        var entity = generator.CreateEntityWithMetadata(amountMdFor, amountOther);
        Equal(amountMdFor + amountOther, entity.Metadata.Count());
    }

}