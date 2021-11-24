namespace ToSic.Eav.Data
{
    public  partial class ContentType
    {

        #region Sharing Content Types
        ///// <inheritdoc />
        //public int? ParentId { get; internal set; }
        ///// <inheritdoc />
        //public int ParentAppId { get; }
        ///// <inheritdoc />
        //public int ParentZoneId { get; }
        /// <inheritdoc />
        public bool AlwaysShareConfiguration { get; private set; }

        #endregion
    }
}
