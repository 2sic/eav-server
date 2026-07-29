using ToSic.Eav.Data;

namespace ToSic.Eav.Models;

public class ModelSetupNullTests
{
    #region Test Data

    private class MockModelNullCapable : IModelFromEntity, IModelSetup<IEntity>
    {

        bool IModelSetup<IEntity>.SetupModel(IEntity? source)
        {
            return true;
        }

    }

    private class MockModelNullUnCapable : IModelFromEntity, IModelSetup<IEntity>
    {

        bool IModelSetup<IEntity>.SetupModel(IEntity? source)
        {
            return false;
        }

    }

    #endregion


    #region Helpers

    private static TModel? CreateAndSetup<TModel>(ModelNullHandling nullHandling, IEntity? data = null)
        where TModel : class, IModelSetup<IEntity>, new()
    {
        var x = new TModel();
        return x.SetupWithDataNullChecks(data, nullHandling);
    }

    #endregion


    #region Setup with Null Variants

    [Fact]
    public void NullWithDataAsNull_Capable_IsNull() =>
        Null(CreateAndSetup<MockModelNullCapable>(ModelNullHandling.DataNullAsNull));

    [Fact]
    public void NullWithDataAsNull_UnCapable_IsNull() =>
        Null(CreateAndSetup<MockModelNullUnCapable>(ModelNullHandling.DataNullAsNull));

    [Fact]
    public void NullWithDataAsModelTry_Capable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullCapable>(ModelNullHandling.DataNullTryConvert));

    [Fact]
    public void NullWithDataAsModelTry_UnCapable_IsNull() => 
        Null(CreateAndSetup<MockModelNullUnCapable>(ModelNullHandling.DataNullTryConvert));

    [Fact]
    public void NullWithDataAsModelForce_Capable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullCapable>(ModelNullHandling.DataNullForceConvert));

    [Fact]
    public void NullWithDataAsModelForce_UnCapable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullUnCapable>(ModelNullHandling.DataNullForceConvert));

    [Fact]
    public void NullWithDataAsThrow_Capable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullCapable>(ModelNullHandling.DataNullThrows)
        );

    [Fact]
    public void NullWithDataAsThrow_UnCapable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullUnCapable>(ModelNullHandling.DataNullThrows)
        );

    [Fact]
    public void NullWithDataAsModelOrThrow_Capable_NotNull() =>
        NotNull(CreateAndSetup<MockModelNullCapable>(ModelNullHandling.DataNullTryConvertOrThrow));


    [Fact]
    public void NullWithDataAsModelOrThrow_UnCapable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullUnCapable>(ModelNullHandling.DataNullTryConvertOrThrow)
        );

    #endregion


    #region NullToModel

    [Fact]
    public void NullToModel_DataNullTryToConvert_NotNull() =>
        NotNull(((IEntity)null!).ToModelTac<MockModelNullCapable>(nullHandling: ModelNullHandling.DataNullTryConvert));

    [Fact]
    public void NullToModel_Null() =>
        Null(((IEntity)null!).ToModelTac<MockModelNullUnCapable>());

    [Fact]
    public void NullToModel_DataNullAsNull_Null() =>
        Null(((IEntity)null!).ToModelTac<MockModelNullCapable>(nullHandling: ModelNullHandling.DataNullAsNull));

    #endregion

}
