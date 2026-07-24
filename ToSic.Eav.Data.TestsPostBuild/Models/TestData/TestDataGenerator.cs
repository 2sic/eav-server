using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models.TestData;

public class TestDataGenerator(DataAssembler dataAssembler, ContentTypesFromCodeManager ctDefFactory, ContentTypeAssembler typeAssembler)
{
    public IEntity EntityWithMetadataForDecorator(int amount)
    {
        var original = dataAssembler.TestEntityDaniel(typeAssembler);

        var decorators = CreateMdForDecorators(amount);

        var lazyPartsBuilder = dataAssembler.EntityConnection.UseMetadata(decorators);

        var entity = dataAssembler.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    public IEntity CreateWithMixedMetadata(int amountMdFor, int amountOther)
    {
        var original = dataAssembler.TestEntityDaniel(typeAssembler);

        var decorators = CreateMdForDecorators(amountMdFor);

        var newDecorators = CreateMdEmpty(amountOther);

        var lazyPartsBuilder = dataAssembler.EntityConnection.UseMetadata(decorators.Concat(newDecorators));

        var entity = dataAssembler.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    private IEnumerable<IEntity> CreateMdEmpty(int amountOther)
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
        var ct = ctDefFactory.CreateTac<TestModelMetadataForDecorator>();

        return dataAssembler.CreateEntityTac(0, ct, values: new()
        {
            { nameof(TestModelMetadataForDecorator.Amount), 1 },
            { nameof(TestModelMetadataForDecorator.TargetName), nameof(TargetTypes.Entity) },
            { nameof(TestModelMetadataForDecorator.TargetType), (int)TargetTypes.Entity },
            { nameof(TestModelMetadataForDecorator.DeleteWarning), null! }
        });
    }

    private IEntity CreateEntityForNoSpecs()
    {
        var ct = ctDefFactory.CreateTac<MockEntityType>();

        return dataAssembler.CreateEntityTac(0, ct, values: new()
        {
            { nameof(MockEntityType.Id), 1 },
            { nameof(MockEntityType.Name), "Test" },
            { nameof(MockEntityType.Age), 30 },
            { nameof(MockEntityType.BirthDate), new DateTime(1990, 1, 1) },
            { nameof(MockEntityType.IsAlive), true }
        });
    }

    private class MockEntityType
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int Age { get; set; }

        public DateTime BirthDate { get; set; }

        public bool IsAlive { get; set; }
    }

    #endregion
}
