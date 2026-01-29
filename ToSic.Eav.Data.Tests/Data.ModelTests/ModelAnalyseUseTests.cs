using ToSic.Eav.Models;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.ModelTests;

public class ModelAnalyseUseTests
{
    private void AssertType<TInspect, TExpected>()
        where TInspect : class, IWrapperWip =>
        Equal(typeof(TExpected), DataModelAnalyzerTestAccessors.GetTargetTypeTac<TInspect>());

    #region NotDecorated - should return itself as the type

    class NotDecorated : IWrapperWip; // ICanWrapData;

    [Fact]
    public void NotDecoratedDataModelType() =>
        AssertType<NotDecorated, NotDecorated>();

    #endregion

    #region Interface not Decorated - should return itself as the type

    interface INotDecorated : IWrapperWip; // ICanWrapData;

    [Fact]
    //[ExpectedException(typeof(TypeInitializationException))]
    public void INotDecoratedType() =>
        Throws<TypeInitializationException>(AssertType<INotDecorated, INotDecorated>);

    #endregion

    #region Decorated - should return the decorated type

    [ModelCreation(Use = typeof(DecoratedEntity))]
    record Decorated: ModelOfEntity;

    record DecoratedEntity : Decorated;

    [Fact]
    public void DecoratedType() =>
        AssertType<Decorated, DecoratedEntity>();

    #endregion


    #region Inherit Decorated but not decorated - should return itself as the type

    record InheritDecorated : Decorated;

    [Fact]
    public void InheritDecoratedType() =>
        AssertType<InheritDecorated, InheritDecorated>();

    #endregion

    #region Inherit and redecorate, should return the newly decorated type

    [ModelCreation(Use = typeof(InheritReDecoratedEntity))]
    record InheritReDecorated : InheritDecorated;

    record InheritReDecoratedEntity : InheritReDecorated;

    [Fact]
    public void InheritReDecoratedType() =>
        AssertType<InheritReDecorated, InheritReDecoratedEntity>();

    #endregion

    #region Interface decorated - should return the decorated type

    [ModelCreation(Use = typeof(EntityOfIDecorated))]
    interface IDecorated : IWrapperWip; // ICanWrapData;

    record EntityOfIDecorated : InheritReDecorated, IDecorated;

    [Fact]
    public void IDecoratedType() =>
        AssertType<IDecorated, EntityOfIDecorated>();

    #endregion
}