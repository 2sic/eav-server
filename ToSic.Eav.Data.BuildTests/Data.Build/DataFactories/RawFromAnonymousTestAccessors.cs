using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal static class RawFromAnonymousTestAccessors
{
    public static IRawEntity ConvertTac(this RawFromAnonymousHelper helper, object data)
        => helper.Convert(data);
}
