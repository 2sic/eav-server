using ToSic.Eav.Data;

namespace ToSic.Eav.Models.TestData;

/// <summary>
/// The interface - which when used in ToModel will automatically use the <see cref="MockMetadataModel"/>.
/// </summary>
internal interface IMockMetadataModel : IModelFromEntity<MockMetadataModel>, IModelSetup<IEntity>
{
    public int TargetType { get; }
    public string TargetName { get; }
    public int Amount { get; }
    public string? DeleteWarning { get; }
}


// TODO: CONTINUE HERE

// TODO: run tests, this should fail as it doesn't have the IModelFromEntity<MockMetadataModel> interface
internal interface IMockMetadataModelDerived: IMockMetadataModel;

// TODO: run tests, this should work
internal interface IMockMetadataModelDerivedReApplyingInterface: IMockMetadataModel, IModelFromEntity<MockMetadataModel>;

// TODO: RUN TESTS, THIS SHOULD FAIL
internal interface IMockMetadataModelDerivedReApplyingInterfaceForInterface: IMockMetadataModel, IModelFromEntity<IMockMetadataModel>;

// TODO: ADD BAD SPECS, THIS SHOULD FAIL
[ModelSpecs(ContentType = "WRONG NAME")]
internal interface IMockMetadataModelDerivedReApplyingInterfaceWithSpecs : IMockMetadataModel, IModelFromEntity<MockMetadataModel>;