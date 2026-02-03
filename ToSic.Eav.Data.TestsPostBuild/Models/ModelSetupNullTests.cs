using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class ModelSetupNullTests
{
    public TModel? Create<TModel>(ModelNullHandling nullHandling, IEntity? data = null)
        where TModel : class, IModelSetup<IEntity>, new()
    {
        var x = new TModel();
        return x.SetupWithDataNullChecks(data, nullHandling);
    }

    [Fact]
    public void NullWithDataAsNull_Capable() =>
        Null(Create<TestModelNullCapable>(ModelNullHandling.DataNullAsNull));

    [Fact]
    public void NullWithDataAsNull_UnCapable() =>
        Null(Create<TestModelNullUnCapable>(ModelNullHandling.DataNullAsNull));

    [Fact]
    public void NullWithDataAsModelTry_Capable() => 
        NotNull(Create<TestModelNullCapable>(ModelNullHandling.DataNullTryConvert));

    [Fact]
    public void NullWithDataAsModelTry_UnCapable() => 
        Null(Create<TestModelNullUnCapable>(ModelNullHandling.DataNullTryConvert));

    [Fact]
    public void NullWithDataAsModelForce_Capable() => 
        NotNull(Create<TestModelNullCapable>(ModelNullHandling.DataNullForceConvert));

    [Fact]
    public void NullWithDataAsModelForce_UnCapable() => 
        NotNull(Create<TestModelNullUnCapable>(ModelNullHandling.DataNullForceConvert));

    [Fact]
    public void NullWithDataAsThrow_Capable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullCapable>(ModelNullHandling.DataNullThrows)
        );

    [Fact]
    public void NullWithDataAsThrow_UnCapable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullUnCapable>(ModelNullHandling.DataNullThrows)
        );

    [Fact]
    public void NullWithDataAsModelOrThrow_Capable() =>
        NotNull(Create<TestModelNullCapable>(ModelNullHandling.DataNullTryConvertOrThrow));


    [Fact]
    public void NullWithDataAsModelOrThrow_UnCapable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullUnCapable>(ModelNullHandling.DataNullTryConvertOrThrow)
        );

}
