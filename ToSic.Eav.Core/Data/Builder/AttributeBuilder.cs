using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public partial class AttributeBuilder: ServiceBase
    {
        #region Dependency Injection

        public AttributeBuilder(ValueBuilder valueBuilder, DimensionBuilder languageBuilder) : base("Dta.AttBld")
        {
            ConnectServices(
                _languageBuilder = languageBuilder,
                ValueBuilder = valueBuilder
            );

        }
        protected readonly ValueBuilder ValueBuilder;
        private readonly DimensionBuilder _languageBuilder;

        #endregion

    }
}
