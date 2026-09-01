using ToSic.Eav.Models.Factory;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Data.Models.Sys;

// ReSharper disable once InconsistentNaming
public class ModelAnalysers_DetectsExpectedEntityModelTargetType
{
    #region Helper to test input and expected

    private static void AssertEntityTargetType<TInspect, TExpected>()
        where TInspect : class, IModelFromEntity
    {
        var data = ModelAnalysersTac.GetTargetTypeTac<TInspect>();
        Equal(typeof(TExpected), data);
    }
    
    private static void AssertEntityTargetTypeNoFactory<TInspect, TExpected>()
        where TInspect : class, IModelFromEntity
    {
        var data = ModelAnalysersTac.GetTargetTypeNoFactoryTac<TInspect>("TestMethod");
        Equal(typeof(TExpected), data);
    }

    private static void CreateNoFactory<TInspect>()
        where TInspect : class, IModelFromEntity
        => ModelAnalysersTac.GetTargetTypeNoFactoryTac<TInspect>("TestMethod");

    #endregion

    #region Basic Class, Can't work because it doesn't implement the IModelFromEntity interface, commented out

    //private class EmptyClass;

    //[Fact]
    //public void EmptyClass_NoFac_ThrowsMissignSetup() =>
    //    Throws<MissingSetupException>(AssertEntityTargetTypeNoFactory<EmptyClass, EmptyClass>);
    
    #endregion

    #region Class NotDecorated - should return itself as the type

    // ReSharper disable once ClassNeverInstantiated.Local
    private class NotDecorated : IModelFromEntity;

    [Fact]
    public void TypeUndecorated_Get_ReturnsItself() =>
        AssertEntityTargetType<NotDecorated, NotDecorated>();

    [Fact]
    public void TypeUndecorated_NoFac_ThrowsMissignSetup() =>
        Throws<MissingSetupException>(CreateNoFactory<NotDecorated>);

    #endregion

    #region Class With Constructor - should throw

    // ReSharper disable once ClassNeverInstantiated.Local
    private class WithConstructor(string Test) : IModelFromEntity;

    [Fact]
    public void TypeWithConstructor_Get_Works() =>
        AssertEntityTargetType<WithConstructor, WithConstructor>();

    [Fact]
    public void TypeWithConstructor_NoFac_Throws() =>
        Throws<MissingConstructorException>(CreateNoFactory<WithConstructor>);

    #endregion
    
    #region Class Requiring Factory

    // ReSharper disable once ClassNeverInstantiated.Local
    private class RequiresFactory : IModelFromEntity, IModelFactoryRequired;

    [Fact]
    public void RequiresFactory_Get_ReturnsItself() =>
        AssertEntityTargetType<RequiresFactory, RequiresFactory>();

    [Fact]
    public void RequiresFactory_NoFac_ThrowsMissignFactory() =>
        Throws<MissingFactoryException>(CreateNoFactory<RequiresFactory>);

    #endregion
    
    #region Interface not Decorated - should throw

    private interface INotDecorated : IModelFromEntity;

    [Fact]
    public void InterfaceUndecorated_Get_Throws() =>
        Throws<TypeInitializationException>(AssertEntityTargetType<INotDecorated, INotDecorated>);
    
    [Fact]
    public void InterfaceUndecorated_NoFac_Throws() =>
        Throws<TypeInitializationException>(CreateNoFactory<INotDecorated>);
    
    #endregion

    
    #region Class Decorated - should return the decorated type

    //[ModelSpecs(Use = typeof(DecoratedEntity))]
    private record Decorated: ModelFromEntity, IModelFromEntity<DecoratedEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record DecoratedEntity : Decorated;

    [Fact]
    public void TypeDecorated_Get_ReturnsModelSpecUse() =>
        AssertEntityTargetType<Decorated, DecoratedEntity>();
    
    [Fact]
    public void TypeDecorated_NoFac_ReturnsModelSpecUse() =>
        AssertEntityTargetTypeNoFactory<Decorated, DecoratedEntity>();

    #endregion


    #region Inherit Decorated but not decorated - should return itself as the type

    private record InheritDecorated : Decorated;

    [Fact]
    public void TypeUndecorated_InheritFromDecorated_Get_ReturnsItself() =>
        AssertEntityTargetType<InheritDecorated, InheritDecorated>();
    
    [Fact]
    public void TypeUndecorated_InheritFromDecorated_NoFac_ReturnsItself() =>
        AssertEntityTargetTypeNoFactory<InheritDecorated, InheritDecorated>();
    
   

    #endregion

    
    #region Inherit and redecorate, should return the newly decorated type

    private record InheritReDecorated : InheritDecorated, IModelFromEntity<InheritReDecoratedEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record InheritReDecoratedEntity : InheritReDecorated;

    [Fact]
    public void TypeDecorated_InheritFromDecorated_Get_ReturnsNewlyDecoratedType() =>
        AssertEntityTargetType<InheritReDecorated, InheritReDecoratedEntity>();

    [Fact]
    public void TypeDecorated_InheritFromDecorated_NoFac__ReturnsNewlyDecoratedType() =>
        AssertEntityTargetTypeNoFactory<InheritReDecorated, InheritReDecoratedEntity>();

   #endregion

    
    #region Interface decorated - should return the decorated type

    private interface IDecorated : IModelFromEntity<EntityOfIDecorated>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record EntityOfIDecorated : InheritReDecorated, IDecorated;

    [Fact]
    public void InterfaceDecorated_Get_ReturnsModelSpecsUse() =>
        AssertEntityTargetType<IDecorated, EntityOfIDecorated>();

    [Fact]
    public void InterfaceDecorated_NoFac_ReturnsModelSpecsUse() =>
        AssertEntityTargetTypeNoFactory<IDecorated, EntityOfIDecorated>();
   #endregion


    #region Interface with ModelFromEntity<T>

    private interface IUndecoratedWithInheritance : IModelFromEntity<UndecoratedWithInheritanceModelFromEntity>;

    // ReSharper disable once ClassNeverInstantiated.Local
    private record UndecoratedWithInheritanceModelFromEntity : IUndecoratedWithInheritance;

    [Fact]
    public void InterfaceInheritingDef_Get_ReturnsModelSpecsUse() =>
        AssertEntityTargetType<IUndecoratedWithInheritance, UndecoratedWithInheritanceModelFromEntity>();

    [Fact]
    public void InterfaceInheritingDef_NoFac_ThrowsMissingSetup() =>
        Throws<MissingSetupException>(CreateNoFactory<IUndecoratedWithInheritance>);
   #endregion
}