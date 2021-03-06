﻿using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Anything with this interface has a property `Metadata` which can give us more
    /// information about that object. 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IHasMetadata
    {
        /// <summary>
        /// Additional information, specs etc. about this attribute
        /// </summary>
        IMetadataOf Metadata { get; }
    }
}
