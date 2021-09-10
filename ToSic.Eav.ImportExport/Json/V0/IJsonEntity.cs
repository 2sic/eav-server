using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ImportExport.Json.V0
{
    /// <summary>
    /// The First and most basic JSON format of EAV.
    /// It's a simple dictionary with name-value pairs.
    /// It is for export/serialization only, there is no official way to re-import an entity of this type.
    ///
    /// It is only meant to hold values of one language.
    /// 
    /// Note that keys are always <see cref="StringComparer.InvariantCultureIgnoreCase"/>
    /// </summary>
    /// <remarks>
    /// Introduced ca. 2sxc 4.0 just as a Dictionary, but for the documentation we created an own IJsonEntity type in 2sxc 12.05
    /// </remarks>
    [PublicApi]
    public interface IJsonEntity: IDictionary<string, object>
    {
        string TestValue27 { get; }
    }
}
