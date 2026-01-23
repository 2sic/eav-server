using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.ExtensionsTests.TestData;

public class TestDataGenerator(DataBuilder builder, ContentTypeFactory ctFactory)
{
    public IEntity EntityWithMetadataForDecorator(int amount)
    {
        var original = builder.TestEntityDaniel();

        var decorators = CreateMdForDecorators(amount);

        var lazyPartsBuilder = builder.EntityConnection.UseMetadata(decorators);

        var entity = builder.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    public IEntity CreateWithMixedMetadata(int amountMdFor, int amountOther)
    {
        var original = builder.TestEntityDaniel();

        var decorators = CreateMdForDecorators(amountMdFor);

        var newDecorators = CreateMdEmpty(amountOther);

        var lazyPartsBuilder = builder.EntityConnection.UseMetadata(decorators.Concat(newDecorators));

        var entity = builder.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    public IEnumerable<IEntity> CreateMdEmpty(int amountOther)
    {
        var mdEmptyDecorator = CreateEntityForNoSpecs();
        var newDecorators = Enumerable
            .Range(1, amountOther)
            .Select(i => mdEmptyDecorator);
        return newDecorators;
    }

    public IEnumerable<IEntity> CreateMdForDecorators(int amount)
    {
        var mdForDecorator = CreateMetadataForDecorator();
        var decorators = Enumerable
            .Range(1, amount)
            .Select(i => mdForDecorator);
        return decorators;
    }


    #region Generate One

    public IEntity CreateMetadataForDecorator()
    {
        var ct = ctFactory.CreateTac<TestModelMetadataForDecorator>();

        return builder.CreateEntityTac(0, ct, values: new()
        {
            { nameof(TestModelMetadataForDecorator.Amount), 1 },
            { nameof(TestModelMetadataForDecorator.TargetName), nameof(TargetTypes.Entity) },
            { nameof(TestModelMetadataForDecorator.TargetType), (int)TargetTypes.Entity },
            { nameof(TestModelMetadataForDecorator.DeleteWarning), null! }
        });
    }

    public IEntity CreateEntityForNoSpecs()
    {
        var ct = ctFactory.CreateTac<CodeTypeNoSpecs>();

        return builder.CreateEntityTac(0, ct, values: new()
        {
            { nameof(CodeTypeNoSpecs.Id), 1 },
            { nameof(CodeTypeNoSpecs.Name), "Test" },
            { nameof(CodeTypeNoSpecs.Age), 30 },
            { nameof(CodeTypeNoSpecs.BirthDate), new DateTime(1990, 1, 1) },
            { nameof(CodeTypeNoSpecs.IsAlive), true }
        });
    }


    #endregion
}
