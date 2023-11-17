using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Run
{
    [PrivateApi("internal functionality")]
    public interface IPlatformInfo
    {
        string Name { get; }
        Version Version { get; }
        string Identity { get; }
    }
}
