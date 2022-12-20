﻿using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Interface to add <see cref="ILog"/>s to the log storage.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface ILogStore
    {
        /// <summary>
        /// Add a log to the current history.
        /// </summary>
        /// <param name="segment">Segment name, like `webapi` or `module`</param>
        /// <param name="log"></param>
        void Add(string segment, ILog log);

        [PrivateApi]
        void ForceAdd(string key, ILog log);

    }
}