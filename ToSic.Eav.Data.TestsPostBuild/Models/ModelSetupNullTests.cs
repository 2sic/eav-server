using ToSic.Eav.Data;
using ToSic.Eav.Models.TestData;

namespace ToSic.Eav.Models;

public class ModelSetupNullTests
{
    public TModel? Create<TModel>(NullToModel nullHandling, IEntity? data = null)
        where TModel : class, IModelSetup<IEntity>, new()
    {
        var x = new TModel();
        return x.SetupWithDataNullChecks(data, nullHandling);
    }

    [Fact]
    public void NullWithDataAsNull_Capable() =>
        Null(Create<TestModelNullCapable>(NullToModel.DataAsNull));

    [Fact]
    public void NullWithDataAsNull_UnCapable() =>
        Null(Create<TestModelNullUnCapable>(NullToModel.DataAsNull));

    [Fact]
    public void NullWithDataAsModelTry_Capable() => 
        NotNull(Create<TestModelNullCapable>(NullToModel.DataAsModelTry));

    [Fact]
    public void NullWithDataAsModelTry_UnCapable() => 
        Null(Create<TestModelNullUnCapable>(NullToModel.DataAsModelTry));

    [Fact]
    public void NullWithDataAsModelForce_Capable() => 
        NotNull(Create<TestModelNullCapable>(NullToModel.DataAsModelForce));

    [Fact]
    public void NullWithDataAsModelForce_UnCapable() => 
        NotNull(Create<TestModelNullUnCapable>(NullToModel.DataAsModelForce));

    [Fact]
    public void NullWithDataAsThrow_Capable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullCapable>(NullToModel.DataAsThrow)
        );

    [Fact]
    public void NullWithDataAsThrow_UnCapable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullUnCapable>(NullToModel.DataAsThrow)
        );

    [Fact]
    public void NullWithDataAsModelOrThrow_Capable() =>
        NotNull(Create<TestModelNullCapable>(NullToModel.DataAsModelOrThrow));


    [Fact]
    public void NullWithDataAsModelOrThrow_UnCapable() =>
        Throws<InvalidCastException>(() =>
            Create<TestModelNullUnCapable>(NullToModel.DataAsModelOrThrow)
        );

}
