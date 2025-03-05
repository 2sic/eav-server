using System.Runtime.CompilerServices;

namespace ToSic.Lib.Core.Tests;

public abstract class LibTestBase
{
#pragma warning disable CS8625
    protected string ThisMethodName([CallerMemberName] string cName = default) => cName;
#pragma warning restore CS8625

}