using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSourceBase
    {
        #region Various provide-out commands - all PrivateApi

        [PrivateApi]
        [Obsolete]
        public void Provide(GetIEnumerableDelegate getList)
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        [Obsolete("Should never be deleted, but avoid using this - prefer the ImmutableList/Array")]
        public void Provide(string name, GetIEnumerableDelegate getList)
            => Out.Add(name, new DataStream(this, name, getList));

        [PrivateApi]
        public void Provide(GetImmutableListDelegate getList)
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        public void Provide(GetImmutableArrayDelegate getList)
            => Provide(Constants.DefaultStreamName, getList);

        [PrivateApi]
        public void Provide(string name, GetImmutableListDelegate getList)
            => Out.Add(name, new DataStream(this, name, getList));

        [PrivateApi]
        public void Provide(string name, GetImmutableArrayDelegate getList)
            => Out.Add(name, new DataStream(this, name, getList));

        #endregion


    }
}
