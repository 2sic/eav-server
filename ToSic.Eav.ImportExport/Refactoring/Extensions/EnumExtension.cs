namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    // TODO2tk: This class can be used in the user interface to convert an enumeration value to a readable string... 
    // it can be moved to the 2sxc project along with the ImportErrorCode.xx.resx files
    public static class EnumExtensions
    {
        /// <summary>
        /// Get the description of an enumeration value. For that, the enumeration value must have a 
        /// description attribute. 
        /// 
        /// [Description("Human readable text of MyValue1."]
        /// MyValue1
        /// </summary>
        //public static string GetDescription(this Enum enumValue)
        //{
        //    var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

        //    var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        //    if (attributes != null && attributes.Length > 0)
        //    {
        //        return attributes[0].Description;
        //    }
        //    return enumValue.ToString();
        //}
    }
}