using ToSic.Eav.Data;
using ToSic.Sys.Utils.TypeFactoryTests;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.NullHandlingTests;

public class NullHandlingSetupTests : NullHandlingBase
{
    #region SetupWithDataNullChecks Helpers

    private static TModel? CreateAndSetupWithNull<TModel>(NullHandling nullHandling)
        where TModel : class, IModelSetup<IEntity>, new()
    {
        var x = new TModel();
        return x.SetupWithDataNullChecks((IEntity?)null, nullHandling);
    }

    private static object? CreateAndSetupWithNull(Type type, NullHandling nullHandling)
    {
        var x = TypeFactoryTac.CreateInstanceTac(type) as IModelSetup<IEntity>;
        return x!.SetupWithDataNullChecks((IEntity?)null, nullHandling);
    }

    #endregion


    #region Setup with Null Data - Default, ReturnNull or Throw - always does the same as the Setup is never called

    [Theory]
    [InlineData(typeof(MockModelNullDataOk))]
    [InlineData(typeof(MockModelNullDataRejected))]
    [InlineData(typeof(MockModelNullDataThrow))]
    public void JustSetup_Default_Always_IsNull(Type type) =>
        Null(CreateAndSetupWithNull(type, NullHandling.Default));

    [Theory]
    [InlineData(typeof(MockModelNullDataOk))]
    [InlineData(typeof(MockModelNullDataRejected))]
    [InlineData(typeof(MockModelNullDataThrow))]
    public void JustSetup_ReturnNull_Always_IsNull(Type type) =>
        Null(CreateAndSetupWithNull(type, NullHandling.ReturnNull));

    [Theory]
    [InlineData(typeof(MockModelNullDataOk))]
    [InlineData(typeof(MockModelNullDataRejected))]
    [InlineData(typeof(MockModelNullDataThrow))]
    public void JustSetup_Throw_Always_Throws(Type type) =>
        Throws<ArgumentNullException>(() =>
            CreateAndSetupWithNull(type, NullHandling.Throw)
        );

    #endregion



    #region ReturnModel


    [Theory]
    [InlineData(typeof(MockModelNullDataOk))]
    [InlineData(typeof(MockModelNullDataRejected))]
    public void JustSetup_ReturnModel_Typical_NotNull(Type type) =>
        NotNull(CreateAndSetupWithNull(type, NullHandling.ReturnModel));

    [Fact]
    public void JustSetup_ReturnModel_ModelThrow_Throws() =>
        Throws<CustomException>(() =>
            CreateAndSetupWithNull<MockModelNullDataThrow>(NullHandling.ReturnModel)
        );

    #endregion



    #region TryOrNull

    [Fact]
    public void JustSetup_TryOrNull_ModelCapable_NotNull() =>
        NotNull(CreateAndSetupWithNull<MockModelNullDataOk>(NullHandling.TryOrNull));

    [Fact]
    public void JustSetup_TryOrNull_ModelReject_IsNull() =>
        Null(CreateAndSetupWithNull<MockModelNullDataRejected>(NullHandling.TryOrNull));

    [Fact]
    public void JustSetup_TryOrNull_ModelThrow_Throws() =>
        Throws<CustomException>(() =>
            CreateAndSetupWithNull<MockModelNullDataThrow>(NullHandling.TryOrNull)
        );

    #endregion



    #region TryOrThrow

    [Fact]
    public void JustSetup_TryOrThrow_ModelCapable_NotNull() =>
        NotNull(CreateAndSetupWithNull<MockModelNullDataOk>(NullHandling.TryOrThrow));


    [Fact]
    public void JustSetup_TryOrThrow_ModelReject_Throws() =>
        Throws<ArgumentNullException>(() =>
            CreateAndSetupWithNull<MockModelNullDataRejected>(NullHandling.TryOrThrow)
        );

    [Fact]
    public void JustSetup_TryOrThrow_ModelThrow_Throws() =>
        Throws<CustomException>(() =>
            CreateAndSetupWithNull<MockModelNullDataThrow>(NullHandling.TryOrThrow)
        );

    #endregion

}