using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Context;

public static class CultureHelpers
{
    public static CultureInfo SafeCultureInfo(string[] dimensions)
    {
        try
        {
            if (dimensions == null || dimensions.Length == 0)
                return CultureInfo.CurrentCulture;
            var d = dimensions.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(d))
                return CultureInfo.GetCultureInfo(d);
        }
        catch { /* ignore */}

        return CultureInfo.CurrentCulture;
    }

    internal static CultureInfo ThreadCurrentCultureInfo => CultureInfo.CurrentCulture;

}