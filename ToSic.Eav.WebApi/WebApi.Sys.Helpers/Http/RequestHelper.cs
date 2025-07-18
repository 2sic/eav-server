﻿#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ToSic.Eav.WebApi.Sys.Helpers.Http;

/// <summary>
/// Helper to get header, query string and route information from current request.
/// Used as input to build current context.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RequestHelper(IHttpContextAccessor httpContextAccessor)
{
    public T GetTypedHeader<T>(string headerName, T fallback)
    {
        var valueString = httpContextAccessor.HttpContext?.Request.Headers[headerName] ?? StringValues.Empty;
        return ReturnTypedResultOrFallback(valueString, fallback);
    }

    public T GetQueryString<T>(string key, T fallback)
    {
        var valueString = httpContextAccessor.HttpContext?.Request.Query[key] ?? StringValues.Empty;
        return ReturnTypedResultOrFallback(valueString, fallback);
    }

    public T GetRouteValuesString<T>(string key, T fallback)
    {
        // TODO: stv - this looks wrong, don't think valueString is of this type
        var valueString = $"{httpContextAccessor.HttpContext?.Request.RouteValues[key]}";
        return ReturnTypedResultOrFallback(valueString, fallback);
    }

    // TODO: REVIEW IF we should call ObjectExtensions.ChangeTypeOrFallback(...) instead; functionality may be a tiny bit different
    private static T ReturnTypedResultOrFallback<T>(StringValues valueString, T fallback)
    {
        if (valueString == StringValues.Empty) return fallback;
        try
        {
            return (T)Convert.ChangeType(valueString.ToString(), typeof(T));
        }
        catch
        {
            return fallback;
        }
    }

    public int TryGetId(string key) => GetTypedHeader(key, GetQueryString(key, GetRouteValuesString(key, Eav.Sys.EavConstants.NullId)));

}

#endif