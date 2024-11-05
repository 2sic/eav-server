using ToSic.Eav.Data.Build;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Data.ContentTypeFactoryTests;

public class ContentTypeFactoryTestsBase : TestBaseEavCore
{
    protected ContentTypeFactory Factory() => GetService<ContentTypeFactory>();
}
