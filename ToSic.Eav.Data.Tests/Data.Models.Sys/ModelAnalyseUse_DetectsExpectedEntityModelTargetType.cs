using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Models.Sys;

// ReSharper disable once InconsistentNaming
public class ModelAnalyseUse_DetectsExpectedEntityModelTargetType
{
    #region Helper to test input and expected

    private static void AssertEntityTargetType<TInspect, TExpected>()
        where TInspect : class, IModelFromEntity
    {
        var data = DataModelAnalyzerTestAccessors.GetTargetTypeTac<TInspect>();
        Equal(typeof(TExpected), data);
    }
    
    #endregion


    #region NotDecorated - should return itself as the type

    // ReSharper disable once ClassNeverInstantiated.Local
    private class NotDecorated : IModelFromEntity;

    [Fact]
    public void TypeUndecorated_ReturnsItself() =>
        AssertEntityTargetType<NotDecorated, NotDecorated>();

    #endregion

    
    #region Interface not Decorated - should return itself as the type

    private interface INotDecorated : IModelFromEntity;

    [Fact]
    public void InterfaceUndecorated_Throws() =>
        Throws<TypeInitializationException>(AssertEntityTargetType<INotDecorated, INotDecorated>);

    #endregion

    
    #region Decorated - should return the decorated type

    //[ModelSpecs(Use = typeof(DecoratedEntity))]
    private record Decorated: ModelFromEntity, IModelFromEntity<DecoratedEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record DecoratedEntity : Decorated;

    [Fact]
    public void TypeDecorated_ReturnsModelSpecUse() =>
        AssertEntityTargetType<Decorated, DecoratedEntity>();

    #endregion


    #region Inherit Decorated but not decorated - should return itself as the type

    private record InheritDecorated : Decorated;

    [Fact]
    public void TypeUndecorated_InheritFromDecorated_ReturnsItself() =>
        AssertEntityTargetType<InheritDecorated, InheritDecorated>();

    #endregion

    
    #region Inherit and redecorate, should return the newly decorated type

    //[ModelSpecs(Use = typeof(InheritReDecoratedEntity))]
    private record InheritReDecorated : InheritDecorated, IModelFromEntity<InheritReDecoratedEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record InheritReDecoratedEntity : InheritReDecorated;

    [Fact]
    public void TypeDecorated_InheritFromDecorated__ReturnsNewlyDecoratedType() =>
        AssertEntityTargetType<InheritReDecorated, InheritReDecoratedEntity>();

    #endregion

    
    #region Interface decorated - should return the decorated type

    //[ModelSpecs(Use = typeof(EntityOfIDecorated))]
    private interface IDecorated : IModelFromEntity<EntityOfIDecorated>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record EntityOfIDecorated : InheritReDecorated, IDecorated;

    [Fact]
    public void InterfaceDecorated_ReturnsModelSpecsUse() =>
        AssertEntityTargetType<IDecorated, EntityOfIDecorated>();

    #endregion


    #region Interface with ModelFromEntity<T>

    private interface IUndecoratedWithInheritance : IModelFromEntity<UndecoratedWithInheritanceModelFromEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record UndecoratedWithInheritanceModelFromEntity : IUndecoratedWithInheritance;

    [Fact]
    public void InterfaceInheritingDef_ReturnsModelSpecsUse() =>
        AssertEntityTargetType<IUndecoratedWithInheritance, UndecoratedWithInheritanceModelFromEntity>();

    #endregion
}