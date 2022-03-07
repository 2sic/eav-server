namespace ToSic.Eav.Data
{
    public static class ContentTypeAttributeExtensions
    {
        /// <summary>
        /// Special accessor for saving, as the SortOrder is otherwise internal only
        /// </summary>
        /// <param name="attDef"></param>
        /// <param name="sortOrder"></param>
        public static void SetSortOrder(this ContentTypeAttribute attDef, int sortOrder) => attDef.SortOrder = sortOrder;
    }
}
