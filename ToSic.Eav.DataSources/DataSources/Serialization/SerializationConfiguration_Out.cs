using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    public partial class SerializationConfiguration: IDeferredDataSource
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

        /// <inheritdoc />
        [PrivateApi]
        public IDataStream DeferredOut(string name)
            => _out.ContainsKey(name) ? _out[name] : AttachDeferredStreamToOut(name);
        
        
        private readonly IDictionary<string, IDataStream> _out = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);
        private bool _requiresRebuildOfOut = true;

        /// <summary>
        /// Attach all missing streams, now that Out is used the first time.
        /// Note that some streams were already added because of the DeferredOut
        /// </summary>
        private void CreateOutWithAllStreams()
        {
            foreach (var dataStream in In.Where(s => !_out.ContainsKey(s.Key)))
                AttachDeferredStreamToOut(dataStream.Key);
        }


        private IDataStream AttachDeferredStreamToOut(string name)
        {
            var outStream = new DataStream(this, name, () => GetList(name), true);
            _out.Add(name, outStream);
            return outStream;
        }

        #endregion
    }
}
