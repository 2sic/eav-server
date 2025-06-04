using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.PropertyLookup;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasPropLookup
{
    [PrivateApi]
    IPropertyLookup PropertyLookup { get; }
}