using ToSic.Eav.Logging;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.Mocks
{

    public class MockGetLanguage : IGetDefaultLanguage
    {
        public string DefaultLanguage => "en-US";

    }
}
