using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;

namespace ToSic.Eav.Models.TestData;

public class TestDataGenerator(DataAssembler dataAssembler, ContentTypesFromCodeManager ctDefFactory, ContentTypeAssemblyKit ctAssemblyKit)
{
    /// <summary>
    /// Create an entity having metadata - some of the expected main type, others to optionally mix in (for type testing)
    /// </summary>
    /// <param name="amountMdFor"></param>
    /// <param name="amountOther"></param>
    /// <returns></returns>
    public IEntity CreateEntityWithMetadata(int amountMdFor, int amountOther = 0)
    {
        var original = dataAssembler.TestEntityDaniel(ctAssemblyKit);

        var decorators = CreateMany(amountMdFor, CreateMetadataForDecorator);

        var newDecorators = CreateMany(amountOther, CreateEntityForNoSpecs);

        var lazyPartsBuilder = dataAssembler.EntityConnection.UseMetadata(decorators.Concat(newDecorators));

        var entity = dataAssembler.Entity.CreateFrom(original, partsBuilder: lazyPartsBuilder);
        return entity;
    }

    private static IEnumerable<IEntity> CreateMany(int amount, Func<IEntity> entityFactory)
    {
        var mdEmptyDecorator = entityFactory();
        var newDecorators = Enumerable
            .Range(1, amount)
            .Select(i => mdEmptyDecorator);
        return newDecorators;
    }

    #region Generate One

    public IEntity CreateMetadataForDecorator() =>
        CreateMetadataForDecorator(1);
    
    public IEntity CreateMetadataForDecorator(int amount) =>
        dataAssembler.CreateEntityTac(
            0,
            ctDefFactory.CreateTac<MockModelMetadataForDecorator>(),
            values: new MockModelMetadataForDecoratorRaw(amount).Values.ToDictionary(x => x.Key, x => x.Value)!
        );

    private IEntity CreateEntityForNoSpecs() =>
        dataAssembler.CreateEntityTac(
            0,
            ctDefFactory.CreateTac<MockEntityType>(),
            values: new MockEntityType().GetValues()
        );

    private class MockEntityType
    {
        public int Id => 1;

        public string Name => "Test";

        public int Age => 30;

        public DateTime BirthDate => new DateTime(1990, 1, 1);

        public bool IsAlive => true;

        public Dictionary<string, object> GetValues() => new()
        {
            { nameof(Id), Id },
            { nameof(Name), Name },
            { nameof(Age), Age },
            { nameof(BirthDate), BirthDate },
            { nameof(IsAlive), IsAlive }
        };
    }

    #endregion
}
