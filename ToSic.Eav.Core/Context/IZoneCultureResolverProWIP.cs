using System.Collections.Generic;

namespace ToSic.Eav.Context;

public interface IZoneCultureResolverProWIP
{
    List<string> CultureCodesWithFallbacks { get; }
}