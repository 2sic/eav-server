using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        #region Various provide-out commands - all PrivateApi

        [PrivateApi]
        protected void Provide(GetIEnumerableDelegate getList)
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        protected void Provide(string name, GetIEnumerableDelegate getList)
            => Out.Add(name, new DataStream(this, name, getList));


        [PrivateApi]
        protected void Provide(GetImmutableListDelegate getList)
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        protected void Provide(string name, GetImmutableListDelegate getList)
            => Out.Add(name, new DataStream(this, name, getList));

        #endregion


    }
}
