using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    [PublicApi_Stable_ForUseInYourCode]
    public interface IHasMetadata
    {
        /// <summary>
        /// Additional information, specs etc. about this attribute
        /// </summary>
        IMetadataOf Metadata { get; }
    }
}
