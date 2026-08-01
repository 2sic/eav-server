using ToSic.Eav.Metadata;

// ReSharper disable InconsistentNaming

// ReSharper disable RedundantExtendsListEntry

namespace ToSic.Eav.Models.Inheritances;

/// <summary>
/// The interface - which when used in ToModel will automatically use the <see cref="MockForInherit"/>.
/// </summary>
internal interface IMockForInherit : IModelFromEntity<MockForInherit>
{
    public int TargetType { get; }
    public string TargetName { get; }
    public int Amount { get; }
    public string? DeleteWarning { get; }
}

/// <summary>
/// Test Sample Model.
/// </summary>
/// <remarks>
/// This is the "main" type which will also be used to define the ContentType.
/// But just using the automatic definition, without attributes.
/// </remarks>
public record MockForInherit
    : ModelFromEntity,
        IMockForInherit,
        IMockForInherit_Implemented,
        IMockForInherit_ReApplyingInterface,
        IMockForInherit_ReApplyingInterfaceWithSpecsBad,
        IMockForInherit_ReApplyingInterfaceWithSpecsGood,
        IMockForInherit_ReApplyingInterfaceWithSpecsAsterisks
{
    public int TargetType => GetThis((int)TargetTypes.None);

    public string TargetName => GetThis(nameof(TargetTypes.None));

    public int Amount => GetThis(1);

    public string? DeleteWarning => GetThis<string?>(null);
}

/// <summary>
/// This derives, but doesn't specify the ContentType, so it will inherit the ContentType from the base class
/// Expectation: throws because name is off.
/// </summary>
public record MockForInheritDerivedBasic : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Works.
/// </summary>
[ModelSpecs(ContentType = nameof(MockForInherit))]
public record MockForInheritDerivedSpecsGood : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Throws.
/// </summary>
[ModelSpecs(ContentType = "WRONG NAME")]
public record MockForInheritDerivedSpecsBad : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Throws.
/// </summary>
[ModelSpecs(ContentType = "*")]
public record MockForInheritDerivedSpecsAsterisks : MockForInherit;

public record MockForInheritAlternative;

/// <summary>
/// this should fail as the <see cref="IMockForInherit"/> does not implement this interface
/// </summary>
internal interface IMockForInherit_NotImplemented : IMockForInherit;

/// <summary>
/// This should work as the <see cref="IMockForInherit"/> implements this interface
/// </summary>
internal interface IMockForInherit_Implemented : IMockForInherit;

internal interface IMockForInherit_ReApplyingInterface : IMockForInherit, IModelFromEntity<MockForInherit>;

internal interface IMockForInherit_ReApplyingInterfaceForInterface : IMockForInherit, IModelFromEntity<IMockForInherit>;

/// <summary>
/// Has incorrect name, should throw.
/// </summary>
[ModelSpecs(ContentType = "WRONG NAME")]
internal interface IMockForInherit_ReApplyingInterfaceWithSpecsBad : IMockForInherit;

// This should work
[ModelSpecs(ContentType = nameof(IMockForInherit))]
internal interface IMockForInherit_ReApplyingInterfaceWithSpecsGood : IMockForInherit;

/// <summary>
/// This should always work.
/// </summary>
[ModelSpecs(ContentType = "*")]
internal interface IMockForInherit_ReApplyingInterfaceWithSpecsAsterisks : IMockForInherit;