using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class AttributeBuilder(ValueBuilder valueBuilder, DimensionBuilder languageBuilder)
    : ServiceBase("Dta.AttBld", connect: [languageBuilder, valueBuilder])
{
    #region Dependency Injection

    protected readonly ValueBuilder ValueBuilder = valueBuilder;

    #endregion

}