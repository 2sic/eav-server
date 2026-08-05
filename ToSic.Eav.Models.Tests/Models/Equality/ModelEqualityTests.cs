using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;
using ToSic.Sys.TestHelpers.Equality;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.
// ReSharper disable file InconsistentNaming

namespace ToSic.Eav.Models.Equality;

/// <summary>
/// Test Equality for Records
/// </summary>
/// <param name="generator"></param>
/// <param name="equalityChecker"></param>
public class ModelEqualityTests_Record(MockDataGenerator generator, EqualityChecker<ModelEqualityTests_Record.MyMockModelRecord> equalityChecker)
    : ModelEqualityTestsBase<ModelEqualityTests_Record.MyMockModelRecord>(generator, equalityChecker)
{
    /// <summary>
    /// Test Sample Model
    /// </summary>
    [ModelSpecs(ContentType = "*")]
    public record MyMockModelRecord : ModelFromEntity
    {
        public int Amount => GetThis(1);
    }

    /// <summary>
    /// This test only makes sense for records.
    /// </summary>
    /// <param name="equalityType"></param>
    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    public void CopyRecordWith_IsEqual(EqualityTypes equalityType)
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<MyMockModelRecord>()!;
        var md2 = md with { };
        equalityChecker.Equal(md, md2, equalityType);
    }
}



/// <summary>
/// Test Equality for Classes
/// </summary>
/// <param name="generator"></param>
/// <param name="equalityChecker"></param>
public class ModelEqualityTests_Classic(MockDataGenerator generator, EqualityChecker<ModelEqualityTests_Classic.MyMockModelClassic> equalityChecker)
    : ModelEqualityTestsBase<ModelEqualityTests_Classic.MyMockModelClassic>(generator, equalityChecker)
{
    /// <summary>
    /// Test Sample Model
    /// </summary>
    [ModelSpecs(ContentType = "*")]
    public class MyMockModelClassic : ModelFromEntityClassic
    {
        public int Amount => GetThis(1);
    }
}



/// <summary>
/// Shared Equality Tests for Models
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <param name="generator"></param>
/// <param name="equalityChecker"></param>
public abstract class ModelEqualityTestsBase<TModel>(MockDataGenerator generator, EqualityChecker<TModel> equalityChecker)
    where TModel: class, IModelFromEntity
{
    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    [InlineData(EqualityTypes.ReferenceEquals)]
    public void Self_IsEqual(EqualityTypes equalityType)
    {
        var md = generator.GetModel<TModel>()!;
        equalityChecker.Equal(md, md, equalityType);
    }

    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    [InlineData(EqualityTypes.ReferenceEquals)]
    public void DifferentEntityInstances_NotEqual(EqualityTypes equalityType)
    {
        var md = generator.GetModel<TModel>()!;
        var md2 = generator.GetModel<TModel>()!;
        equalityChecker.NotEqual(md, md2, equalityType);
    }
    
    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    [InlineData(EqualityTypes.ReferenceEquals)]
    public void DifferentAmount_NotEqual(EqualityTypes equalityType)
    {
        var md = generator.GetModel<TModel>()!;
        var entity2 = generator.CreateMetadataForDecorator(2);
        var md2 = entity2.ToModelTac<TModel>()!;
        equalityChecker.NotEqual(md, md2, equalityType);
    }


    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    public void RepeatToModel_IsEqual(EqualityTypes equalityType)
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<TModel>()!;
        var md2 = entity.ToModelTac<TModel>()!;
        equalityChecker.Equal(md, md2, equalityType);
    }

    [Theory]
    [InlineData(EqualityTypes.AssertEqual)]
    [InlineData(EqualityTypes.OperatorEqual)]
    [InlineData(EqualityTypes.OperatorEqualNegated)]
    [InlineData(EqualityTypes.ObjectEquals)]
    public void EqualsRecast_UsingAsICanBeEntity(EqualityTypes equalityType)
    {
        var entity = generator.CreateMetadataForDecorator();
        var md = entity.ToModelTac<TModel>()!;
        // Recast and then re-create via ICanBeEntity
        var md2 = ((ICanBeEntity)md).ToModelTac<TModel>()!;
        equalityChecker.Equal(md, md2, equalityType);
    }
}
