using System;
using ToSic.Eav.Documentation;

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
