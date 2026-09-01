using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Eav.Models.Factory;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models.WithFactory;

/// <summary>
/// Test Sample Model
/// </summary>
[ModelSpecs(ContentType = nameof(MockMetadataModel))]
public class MockModelRequiringFactoryNoDependencies
    : IModelFromEntity, IModelSetup<IEntity>, IModelFactoryRequired
{
    public bool SetupModel(IEntity? source) => true;
}

/// <summary>
/// This should throw an error because the name doesn't match
/// </summary>
public class MockModelRequiringFactoryNoDependenciesNoSpecs
    : IModelFromEntity, IModelSetup<IEntity>, IModelFactoryRequired
{
    public bool SetupModel(IEntity? source) => true;
}

/// <summary>
/// Test Sample Model
/// </summary>
[ModelSpecs(ContentType = nameof(MockMetadataModel))]
public class MockModelRequiringFactoryWithDependencies(MockModelRequiringFactoryWithDependencies.Dependencies dependency)
    : IModelFromEntity, IModelSetup<IEntity>, IModelFactoryRequired
{
    /// <summary>
    /// Dependencies of this model, which will be injected by the factory
    /// </summary>
    public class Dependencies
    {
        public const string HelloMessage = "Hello from TestModelDependency";
        public string GetSomething() => HelloMessage;
    }

    bool IModelSetup<IEntity>.SetupModel(IEntity? source)
    {
        _entity = source!;
        return true;
    }

    private IEntity _entity = null!;

    public int TargetType => _entity.Get(nameof(TargetType), fallback: (int)TargetTypes.None);

    public string TargetName => _entity.Get(nameof(TargetName), fallback: nameof(TargetTypes.None));

    public int Amount => _entity.Get(nameof(Amount), fallback: 1);

    public string? DeleteWarning => _entity.Get<string>(nameof(DeleteWarning), fallback: null);

    public string SomethingFromDependency => dependency.GetSomething();


}