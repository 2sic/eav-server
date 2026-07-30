using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class ModelEqualityTests(MockDataGenerator generator)
{
    /// <summary>
    /// Test Sample Model
    /// </summary>
    [ModelSpecs(ContentType = "*")]
    private record MockModel : ModelFromEntity
    {
        public int Amount => GetThis(1);
    }

    private static void CheckAllEquals(MockModel md, MockModel md2)
    {
        Equal(md, md2);
        True(md == md2);
        True(md.Equals(md2));
    }
    private static void CheckAllNotEqual(MockModel md, MockModel md2)
    {
        NotEqual(md, md2);
        False(md == md2);
        False(md.Equals(md2));
    }

    [Fact]
    public void Self_IsEqual()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MockModel>()!;
        CheckAllEquals(md, md);
    }
    
    [Fact]
    public void DifferentEntityInstances_NotEqual()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MockModel>()!;
        var entity2 = generator.CreateMetadataForDecorator();
        var md2 = entity2.ToModelTac<MockModel>()!;
        CheckAllNotEqual(md, md2);
    }
    
    [Fact]
    public void DifferentAmount_NotEqual()
    {
        var entity = generator.CreateMetadataForDecorator(1);
        var md = entity.ToModelTac<MockModel>()!;
        var entity2 = generator.CreateMetadataForDecorator(2);
        var md2 = entity2.ToModelTac<MockModel>()!;
        CheckAllNotEqual(md, md2);
    }


    [Fact]
    public void RepeatToModel_IsEqual()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MockModel>()!;
        var md2 = entity.ToModelTac<MockModel>()!;
        CheckAllEquals(md, md2);
    }

    [Fact]
    public void EqualsRecast_UsingAsICanBeEntity()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MockModel>()!;
        // Recast via ICanBeEntity
        var md2 = ((ICanBeEntity)md).ToModelTac<MockModel>()!;
        CheckAllEquals(md, md2);
    }

    [Fact]
    public void CopyRecordWith_IsEqual()
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MockModel>()!;
        var md2 = md with { };
        CheckAllEquals(md, md2);
    }


}
