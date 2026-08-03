using ToSic.Eav.Data;
using ToSic.Eav.Models.Factory;
// ReSharper disable InconsistentNaming

namespace ToSic.Eav.Models.NullHandlingTests;

public class NullHandlingToModelTests_Static : NullHandlingToModelTests
{
    protected override TModel? NullEntityToModel<TModel>(NullHandling nullHandling) where TModel : class
        => ((IEntity)null!).ToModelTac<TModel>(options: new() { NullHandling = nullHandling });
    
}

public class NullHandlingToModelTests_FactoryDirect(IModelFactory modelFactory) : NullHandlingToModelTests
{
    protected override TModel? NullEntityToModel<TModel>(NullHandling nullHandling) where TModel : class
        => modelFactory.Create<IEntity, TModel>(null!, new() { NullHandling = nullHandling });
}

public class NullHandlingToModelTests_FactoryExtension(IModelFactory modelFactory) : NullHandlingToModelTests
{
    protected override TModel? NullEntityToModel<TModel>(NullHandling nullHandling) where TModel : class
        => ((IEntity)null!).ToModelTac<TModel>(factory: modelFactory, options: new() { NullHandling = nullHandling });
}


/// <summary>
/// These tests build upon the <see cref="NullHandlingSetupTests"/> - so if all that works, also verify it's the same calling ToModel
/// </summary>
public abstract class NullHandlingToModelTests : NullHandlingBase
{
    #region ToModel Test Helpers

    protected abstract TModel? NullEntityToModel<TModel>(NullHandling nullHandling)
        where TModel : class, IModelFromEntity;

    #endregion

    
    
    #region Null calling ToModel()

    [Fact]
    public void ToModel_TryOrNull_Ok_NotNull() =>
        NotNull(NullEntityToModel<MockModelNullDataOk>(NullHandling.TryOrNull));

    [Fact]
    public void ToModel_X_Null() =>
        Null(((IEntity)null!).ToModelTac<MockModelNullDataRejected>());

    #endregion


    #region ToModel() Default & ReturnNull

    [Fact]
    public void ToModel_Default_Ok_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataOk>(NullHandling.Default));

    [Fact]
    public void ToModel_Default_Rejected_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataRejected>(NullHandling.Default));

    [Fact]
    public void ToModel_Default_Throw_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataThrow>(NullHandling.Default));

        
    [Fact]
    public void ToModel_ReturnNull_Ok_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataOk>(NullHandling.ReturnNull));

    [Fact]
    public void ToModel_ReturnNull_Rejected_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataRejected>(NullHandling.ReturnNull));

    [Fact]
    public void ToModel_ReturnNull_Throw_IsNull() =>
        Null(NullEntityToModel<MockModelNullDataThrow>(NullHandling.ReturnNull));
        
    #endregion


    #region ToModel() Throw

    [Fact]
    public void ToModel_Throw_Ok_Throws() =>
        Throws<ArgumentNullException>(() =>
            NullEntityToModel<MockModelNullDataOk>(NullHandling.Throw)
        );

    [Fact]
    public void ToModel_Throw_Rejected_Throws() =>
        Throws<ArgumentNullException>(() =>
            NullEntityToModel<MockModelNullDataRejected>(NullHandling.Throw)
        );

    [Fact]
    public void ToModel_Throw_Throw_Throws() =>
        Throws<ArgumentNullException>(() =>
            NullEntityToModel<MockModelNullDataThrow>(NullHandling.Throw)
        );


    #endregion
}