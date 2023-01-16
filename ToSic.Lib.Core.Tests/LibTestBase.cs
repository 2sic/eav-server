using System.Runtime.CompilerServices;

namespace ToSic.Lib.Core.Tests
{
    public abstract class LibTestBase
    {
        protected string ThisMethodName([CallerMemberName] string cName = default) => cName;

    }
}
