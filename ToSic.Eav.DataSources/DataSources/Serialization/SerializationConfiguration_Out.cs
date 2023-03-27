using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource;

namespace ToSic.Eav.DataSources
{
    public partial class SerializationConfiguration //: IDeferredDataSource
    {

        #region Deferred / Dynamic Out

        /// <inheritdoc/>
        public override IDictionary<string, IDataStream> Out
        {
            get
            {
                if (!_requiresRebuildOfOut) return _out;
                CreateOutWithAllStreams();
                _requiresRebuildOfOut = false;
                return _out;
            }
        }

        ///// <inheritdoc />
        //[PrivateApi]
        //public IDataStream DeferredOut(string name)
        //    => _out.ContainsKey(name) ? _out[name] : AttachDeferredStreamToOut(name);


        private readonly IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase);
        private bool _requiresRebuildOfOut = true;

        /// <summary>
        /// Attach all missing streams, now that Out is used the first time.
        /// Note that some streams were already added because of the DeferredOut
        /// </summary>
        private void CreateOutWithAllStreams()
        {
            foreach (var dataStream in In.Where(s => !_out.ContainsKey(s.Key)))
                //Provide(dataStream.Key, () => GetList(dataStream.Key));
                _out.Add(dataStream.Key, new DataStream(this, dataStream.Key, () => GetList(dataStream.Key)));
        }

        #endregion
    }
}
