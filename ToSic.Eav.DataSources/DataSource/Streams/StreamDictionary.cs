﻿using System.Collections.Generic;
using ToSic.Eav.Generics;

namespace ToSic.Eav.DataSources
{
    public class StreamDictionary: DictionaryInvariant<IDataStream>
    {
        internal IDataSource Source;

        public StreamDictionary() { }

        /// <summary>
        /// Re-bundle an existing set of streams for the new Source which will provide it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="streams"></param>
        public StreamDictionary(IDataSource source, IDictionary<string, IDataStream> streams)
        {
            Source = source;
            if (streams == null) return;

            foreach (var stream in streams)
                Add(stream.Key, WrapStream(stream.Key, stream.Value));
        }

        public new void Add(string name, IDataStream stream) =>
            base.Add(name, Source == null ? stream : WrapStream(name, stream));

        private IDataStream WrapStream(string name, IDataStream stream) =>
            new DataStream(Source, name, () => stream.List)
            {
                Scope = stream.Scope
            };
    }
}