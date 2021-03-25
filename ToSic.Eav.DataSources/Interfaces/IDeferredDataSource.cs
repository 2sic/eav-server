namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Marks DataSources which have dynamic Out streams which depend on the In to be fully populated.
    /// In cases like the Cache-All or SerializationConfiguration this is often not yet the case,
    /// so these can provide a wrapper-stream which won't activate till data is pulled. 
    /// </summary>
    internal interface IDeferredDataSource
    {
        /// <summary>
        /// The extra call that can provide a fake/deferred Out ever if the underlying streams don't exist yet.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDataStream DeferredOut(string name);
    }
}
