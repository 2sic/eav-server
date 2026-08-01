using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.TestData;

namespace ToSic.Eav.Models.TestData;

public class MockDataGenerator(
    DataAssembler dataAssembler,
    ContentTypesFromCodeManager ctDefFactory,
    ContentTypeAssemblyKit ctAssemblyKit)
    : MockDataGenerator<MockMetadataModel>(dataAssembler, ctDefFactory, ctAssemblyKit);

public interface IMockMetadataForGenerator
{
    IEntity CreateMetadataForDecorator();
}

public class MockDataGenerator<TMockMetadataModel>(DataAssembler dataAssembler, ContentTypesFromCodeManager ctDefFactory, ContentTypeAssemblyKit ctAssemblyKit) : IMockMetadataForGenerator
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

    #region Generate The Real Data which we actually want to use/analyze

    public IEntity CreateMetadataForDecorator() =>
        CreateMetadataForDecorator(1);
    
    internal IEntity CreateMetadataForDecorator(int amount) =>
        dataAssembler.CreateEntityTac(
            0,
            ctDefFactory.CreateTac<TMockMetadataModel>(),
            values: new MockMetadataRaw(amount).Values.ToDictionary(x => x.Key, x => x.Value)
        );

    #endregion


    #region Dummy data for mixing in, to verify it will be filtered out correctly

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

        public DateTime BirthDate => new(1990, 1, 1);

        public bool IsAlive => true;

        public Dictionary<string, object?> GetValues() => new()
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
