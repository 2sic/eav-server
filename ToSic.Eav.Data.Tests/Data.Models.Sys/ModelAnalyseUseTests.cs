using ToSic.Eav.Models;
using ToSic.Sys.Wrappers;

namespace ToSic.Eav.Data.Models.Sys;

public class ModelAnalyseUseTests
{
    private void AssertType<TInspect, TExpected>()
        where TInspect : class, IWrapperWip =>
        Equal(typeof(TExpected), DataModelAnalyzerTestAccessors.GetTargetTypeTac<TInspect>());

    #region NotDecorated - should return itself as the type

    private class NotDecorated : IWrapperWip; // ICanWrapData;

    [Fact]
    public void NotDecoratedDataModelType() =>
        AssertType<NotDecorated, NotDecorated>();

    #endregion

    #region Interface not Decorated - should return itself as the type

    private interface INotDecorated : IWrapperWip; // ICanWrapData;

    [Fact]
    //[ExpectedException(typeof(TypeInitializationException))]
    public void INotDecoratedType() =>
        Throws<TypeInitializationException>(AssertType<INotDecorated, INotDecorated>);

    #endregion

    #region Decorated - should return the decorated type

    [ModelSpecs(Use = typeof(DecoratedEntity))]
    private record Decorated: ModelOfEntity;

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
    private interface IDecorated : IWrapperWip; // ICanWrapData;

    private record EntityOfIDecorated : InheritReDecorated, IDecorated;

    [Fact]
    public void IDecoratedType() =>
        AssertType<IDecorated, EntityOfIDecorated>();

    #endregion
}