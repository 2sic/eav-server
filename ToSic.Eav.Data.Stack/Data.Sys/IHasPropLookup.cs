namespace ToSic.Eav.Data.Sys;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasPropLookup
{
    [PrivateApi]
    IPropertyLookup PropertyLookup { get; }
}