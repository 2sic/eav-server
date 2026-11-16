#if !NETFRAMEWORK
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;

namespace ToSic.Eav.Web.Http.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class HttpHybrid(IHttpContextAccessor contextAccessor) : IHttp
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public HttpContext Current => field ??= contextAccessor.HttpContext!;

    /// <inheritdoc />
    public HttpRequest Request => Current?.Request!;

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public NameValueCollection QueryStringParams
    {
        get
        {
            if (field != null)
                return field;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Request == null!) 
                return field = [];

            var paramList = new NameValueCollection();
            Request.Query.ToList().ForEach(i => paramList.Add(i.Key, i.Value));
            return field = paramList;
        }
    }

    /// <inheritdoc />
    public string? GetCookie(string cookieName) => Request?.Cookies[cookieName];
}

#endif