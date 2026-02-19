using ToSic.Eav.Models;

namespace ToSic.Eav.Data.Models.Sys;

public class ModelAnalyseUseTests
{
    private static void AssertType<TInspect, TExpected>()
        where TInspect : class, IModelOfEntity
    {
        var data = DataModelAnalyzerTestAccessors.GetTargetTypeTac<TInspect>();
        Equal(typeof(TExpected), data);
    }

    #region NotDecorated - should return itself as the type

    // ReSharper disable once ClassNeverInstantiated.Local
    private class NotDecorated : IModelOfEntity;

    [Fact]
    public void NotDecoratedDataModelType() =>
        AssertType<NotDecorated, NotDecorated>();

    #endregion

    #region Interface not Decorated - should return itself as the type

    private interface INotDecorated : IModelOfEntity;

    [Fact]
    //[ExpectedException(typeof(TypeInitializationException))]
    public void INotDecoratedType() =>
        Throws<TypeInitializationException>(AssertType<INotDecorated, INotDecorated>);

    #endregion

    #region Decorated - should return the decorated type

    [ModelSpecs(Use = typeof(DecoratedEntity))]
    private record Decorated: ModelOfEntityBasic;

    private record DecoratedEntity : Decorated;

    [Fact]
    public void DecoratedType() =>
        AssertType<Decorated, DecoratedEntity>();

    #endregion


    #region Inherit Decorated but not decorated - should return itself as the type

    private record InheritDecorated : Decorated;

    [Fact]
    public void InheritDecoratedType() =>
        AssertType<InheritDecorated, InheritDecorated>();

    #endregion

    #region Inherit and redecorate, should return the newly decorated type

    [ModelSpecs(Use = typeof(InheritReDecoratedEntity))]
    private record InheritReDecorated : InheritDecorated;

    private record InheritReDecoratedEntity : InheritReDecorated;

    [Fact]
    public void InheritReDecoratedType() =>
        AssertType<InheritReDecorated, InheritReDecoratedEntity>();

    #endregion

    #region Interface decorated - should return the decorated type

    [ModelSpecs(Use = typeof(EntityOfIDecorated))]
    private interface IDecorated : IModelOfEntity;

    private record EntityOfIDecorated : InheritReDecorated, IDecorated;

    [Fact]
    public void IDecoratedType() =>
        AssertType<IDecorated, EntityOfIDecorated>();

    #endregion
}