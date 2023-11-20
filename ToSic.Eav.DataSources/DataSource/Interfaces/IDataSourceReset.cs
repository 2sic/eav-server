namespace ToSic.Eav.DataSource
{
    public  interface IDataSourceReset
    {
        /// <summary>
        /// Reset the query, so it can be run again. Requires all params to be set again.
        /// </summary>
        void Reset();

    }
}
