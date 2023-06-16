namespace ToSic.Eav.WebApi.Sys
{
    internal class SpecialField
    {

        public SpecialField(object value, string styles)
        {
            Styles = styles;
            Value = value;
        }
        public object Value { get; }
        public string Styles { get; }

        public static SpecialField Right(object value) => new SpecialField(value, "text-align: right; padding - right: 5px;");
    }
}