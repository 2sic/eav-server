using ToSic.Eav.Data;
using static ToSic.Eav.Models.ToModelOptions;

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

    private static TModel? CreateAndSetup<TModel>(DataNullHandling nullHandling, IEntity? data = null)
        where TModel : class, IModelSetup<IEntity>, new()
    {
        var x = new TModel();
        return x.SetupWithDataNullChecks(data, nullHandling);
    }

    #endregion


    #region Setup with Null Variants

    [Fact]
    public void NullWithDataAsNull_Capable_IsNull() =>
        Null(CreateAndSetup<MockModelNullCapable>(DataNullHandling.AsNull));

    [Fact]
    public void NullWithDataAsNull_UnCapable_IsNull() =>
        Null(CreateAndSetup<MockModelNullUnCapable>(DataNullHandling.AsNull));

    [Fact]
    public void NullWithDataAsModelTry_Capable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullCapable>(DataNullHandling.ConvertTry));

    [Fact]
    public void NullWithDataAsModelTry_UnCapable_IsNull() => 
        Null(CreateAndSetup<MockModelNullUnCapable>(DataNullHandling.ConvertTry));

    [Fact]
    public void NullWithDataAsModelForce_Capable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullCapable>(DataNullHandling.ConvertForce));

    [Fact]
    public void NullWithDataAsModelForce_UnCapable_NotNull() => 
        NotNull(CreateAndSetup<MockModelNullUnCapable>(DataNullHandling.ConvertForce));

    [Fact]
    public void NullWithDataAsThrow_Capable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullCapable>(DataNullHandling.Throw)
        );

    [Fact]
    public void NullWithDataAsThrow_UnCapable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullUnCapable>(DataNullHandling.Throw)
        );

    [Fact]
    public void NullWithDataAsModelOrThrow_Capable_NotNull() =>
        NotNull(CreateAndSetup<MockModelNullCapable>(DataNullHandling.ConvertOrThrow));


    [Fact]
    public void NullWithDataAsModelOrThrow_UnCapable_Throws() =>
        Throws<InvalidCastException>(() =>
            CreateAndSetup<MockModelNullUnCapable>(DataNullHandling.ConvertOrThrow)
        );

    #endregion


    #region NullToModel

    [Fact]
    public void NullToModel_DataNullTryToConvert_NotNull() =>
        NotNull(((IEntity)null!).ToModelTac<MockModelNullCapable>(options: new() { NullHandling = DataNullHandling.ConvertTry }));

    [Fact]
    public void NullToModel_Null() =>
        Null(((IEntity)null!).ToModelTac<MockModelNullUnCapable>());

    [Fact]
    public void NullToModel_DataNullAsNull_Null() =>
        Null(((IEntity)null!).ToModelTac<MockModelNullCapable>(options: new() { NullHandling = DataNullHandling.AsNull }));

    #endregion

}
