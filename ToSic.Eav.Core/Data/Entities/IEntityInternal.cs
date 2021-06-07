using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public interface IEntityInternal
    {
        #region ValueAndType probably new / WIP in 12.02

        [PrivateApi("WIP, internal")]
        Tuple<object, string> ValueAndType(string fieldName, string[] languages);

        #endregion

    }
}
