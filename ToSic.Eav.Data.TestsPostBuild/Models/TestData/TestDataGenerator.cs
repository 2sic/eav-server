using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.CodeContentTypes;
using ToSic.Eav.Data.TestData;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models.TestData;

public class TestDataGenerator(DataAssembler dataAssembler, CodeContentTypesManager ctDefFactory, ContentTypeAssembler typeAssembler)
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
        var ct = ctDefFactory.CreateTac<TestModelMetadataForDecorator>();

        return dataAssembler.CreateEntityTac(0, ct, values: new()
        {
            { nameof(TestModelMetadataForDecorator.Amount), 1 },
            { nameof(TestModelMetadataForDecorator.TargetName), nameof(TargetTypes.Entity) },
            { nameof(TestModelMetadataForDecorator.TargetType), (int)TargetTypes.Entity },
            { nameof(TestModelMetadataForDecorator.DeleteWarning), null! }
        });
    }

    public IEntity CreateEntityForNoSpecs()
    {
        var ct = ctDefFactory.CreateTac<CodeTypeNoSpecs>();

        return dataAssembler.CreateEntityTac(0, ct, values: new()
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
