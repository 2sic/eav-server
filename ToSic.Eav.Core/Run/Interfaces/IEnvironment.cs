﻿using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// Any object implementing this interface can provide the EAV with information about the environment it's running in.
    /// </summary>
    [PrivateApi("this is not yet ready for publishing, as it's unclear what it actually is")]
    public interface IEnvironment: IHasLog<IEnvironment>
    {
        /// <summary>
        /// The current user in the environment. 
        /// </summary>
        IUser User { get; }
    }
}
