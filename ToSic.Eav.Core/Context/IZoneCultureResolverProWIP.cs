namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneCultureResolverProWIP
{
    /// <summary>
    /// WIP - list of cultures of a ZoneResolver, with list of fallbacks by priority.
    /// ATM only implemented in DNN.
    /// </summary>
    /// <remarks>
    /// MUST guarantee to be lower case, no further checking will happen.
    /// </remarks>
    List<string> CultureCodesWithFallbacks { get; }
}