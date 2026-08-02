using ToSic.Eav.Metadata;

// ReSharper disable InconsistentNaming

// ReSharper disable RedundantExtendsListEntry

namespace ToSic.Eav.Models.Inheritances;

public abstract class TestCaseInheritanceBaseAttribute : Attribute
{
    public string? Notes { get; init; }
}

public class TestCase_IsValidAttribute : TestCaseInheritanceBaseAttribute;

public class TestCase_ExpectedTypeAttribute : TestCaseInheritanceBaseAttribute
{
    /// <summary>
    /// Optional expected type - should only be specified if the resulting Model will be different from the type where this attribute is set.
    /// </summary>
    public Type? Type { get; set; }
}

public class TestCase_BadNameAttribute : TestCaseInheritanceBaseAttribute;

public class TestCase_BadInterfaceCastAttribute : TestCaseInheritanceBaseAttribute;

/// <summary>
/// The interface - which when used in ToModel will automatically use the <see cref="MockForInherit"/>.
/// </summary>
[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit))]
internal interface IMockForInherit : IModelFromEntity<MockForInherit>
{
    public int TargetType { get; }
    public int Amount { get; }
}

/// <summary>
/// Test Sample Model.
/// </summary>
/// <remarks>
/// This is the "main" type which will also be used to define the ContentType.
/// But just using the automatic definition, without attributes.
/// </remarks>
[TestCase_IsValid]
[TestCase_ExpectedType]
public record MockForInherit
    : ModelFromEntity,
        IMockForInherit,
        IMockForInherit_Implemented,
        IMockForInherit_ReApplyingInterface,
        IMockForInherit_ReApplyingInterfaceForInterface,
        IMockForInherit_SpecsNameWrong,
        IMockForInherit_SpecsNameExact,
        IMockForInherit_SpecsNameIMockForInherit,
        IMockForInherit_SpecsNameAsterisks
{
    public int TargetType => GetThis((int)TargetTypes.None);

    public int Amount => GetThis(1);
}

/// <summary>
/// This derives, but doesn't specify the ContentType, so it will inherit the ContentType from the base class
/// Expectation: throws because name is off.
/// </summary>
[TestCase_BadName]
public record MockForInheritDerivedBasic : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Works.
/// </summary>
[TestCase_IsValid]
[TestCase_ExpectedType]
[ModelSpecs(ContentType = nameof(MockForInherit))]
public record MockForInheritDerivedSpecsGood : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Throws.
/// </summary>
[TestCase_BadName]
[ModelSpecs(ContentType = "WRONG NAME")]
public record MockForInheritDerivedSpecsBad : MockForInherit;

/// <summary>
/// To derive a model class, we must ensure that the name-checks won't fail
/// Expectation: Throws.
/// </summary>
[TestCase_IsValid] 
[TestCase_ExpectedType]
[ModelSpecs(ContentType = "*")]
public record MockForInheritDerivedSpecsAsterisks : MockForInherit;

public record MockForInheritAlternative: IModelFromEntity;

/// <summary>
/// This should work as the <see cref="IMockForInherit"/> implements this interface
/// </summary>
[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit))] 
internal interface IMockForInherit_Implemented : IMockForInherit;

[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit))]
internal interface IMockForInherit_ReApplyingInterface : IMockForInherit, IModelFromEntity<MockForInherit>;



/// <summary>
/// Implemented, but nothing in the data suggest want concrete type, so it should fail.
/// </summary>
[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit), Notes = "This works even if we set another IModel, because they are compatible")]
internal interface IMockForInherit_ReApplyingInterfaceForInterface : IMockForInherit, IModelFromEntity<IMockForInherit>;


/// <summary>
/// Has incorrect name, should throw.
/// </summary>
[TestCase_BadName]
[ModelSpecs(ContentType = "WRONG NAME")]
internal interface IMockForInherit_SpecsNameWrong : IMockForInherit;

// This should work
[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit))]
[ModelSpecs(ContentType = nameof(MockForInherit))]
internal interface IMockForInherit_SpecsNameExact : IMockForInherit;

/// <summary>
/// This should NOT work if the names is checked.
/// </summary>
[TestCase_BadName]
[ModelSpecs(ContentType = nameof(IMockForInherit))]
internal interface IMockForInherit_SpecsNameIMockForInherit: IMockForInherit;

/// <summary>
/// This should always work.
/// </summary>
[TestCase_IsValid]
[TestCase_ExpectedType(Type = typeof(MockForInherit))]
[ModelSpecs(ContentType = "*")]
internal interface IMockForInherit_SpecsNameAsterisks : IMockForInherit;



/// <summary>
/// this should fail as the <see cref="IMockForInherit"/> does not implement this interface
/// </summary>
[TestCase_BadInterfaceCast(Notes = "This fails even if we set another IModel, because they are compatible")]
internal interface IMockForInherit_NotImplemented : IMockForInherit;

/// <summary>
/// Implemented, but nothing in the data suggest want concrete type, so it should fail.
/// </summary>
[TestCase_BadInterfaceCast(Notes = "This fails even if we set another IModel, because they are compatible")]
internal interface IMockForInherit_ReApplyingInterfaceWrong : IMockForInherit, IModelFromEntity<MockForInheritAlternative>;

