using System.Collections;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavJsonConverterFactory(
    EavJsonConverter eavJsonConverter,
    EavCollectionJsonConverter eavCollectionJsonConverter
#if NETCOREAPP
    , IHttpContextAccessor htppContextAccessor
#endif
        )
    : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(IEntity).IsAssignableFrom(typeToConvert)
        || typeof(IEnumerable<IEntity>).IsAssignableFrom(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // @STV need your help here
//#if !NETCOREAPP
//        // 1. Access the current HttpContext
//        var httpContext = HttpContext.Current;

//        // TEMP - should move to DNN if this works
//        // 2. Use the HttpContext to resolve dependencies - if possible
//        if (httpContext != null)
//        {
//            var scope = httpContext.Items[typeof(IServiceScope)] as IServiceScope;
//            var sp = scope?.ServiceProvider;
//            if (sp != null)
//                return typeof(IEnumerable).IsAssignableFrom(typeToConvert)
//                    ? sp.Build<EavCollectionJsonConverter>()
//                    //: sp.Build<EavJsonConverter>();
//                    : sp.Build<EavJsonConverterHttpEntry>();
//        }
//#endif

        return typeof(IEnumerable).IsAssignableFrom(typeToConvert)
            ? eavCollectionJsonConverter
            : eavJsonConverter;
    }
}