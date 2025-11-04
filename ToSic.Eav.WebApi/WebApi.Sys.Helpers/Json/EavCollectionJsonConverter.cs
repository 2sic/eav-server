using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Web;
using ToSic.Eav.DataFormats.EavLight;

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavCollectionJsonConverter(
    IServiceProvider serviceProvider
#if NETCOREAPP
    , IHttpContextAccessor httpContextAccessor
#endif
        ) : JsonConverter<IEnumerable<IEntity>>
{
    public override IEnumerable<IEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEnumerable<IEntity> entities, JsonSerializerOptions options)
    {
        // Get request-scoped converter to ensure correct culture resolution
        var converter = GetCurrentRequestConverter();
        var eavLightEntities = converter.Convert(entities);
        JsonSerializer.Serialize(writer, eavLightEntities, eavLightEntities.GetType(), options);
    }

    /// <summary>
    /// Get the IConvertToEavLight from the current HTTP request's service scope.
    /// This ensures the converter uses the current request's culture information.
    /// </summary>
    private IConvertToEavLight GetCurrentRequestConverter()
    {
        IServiceProvider? scopedProvider = null;

#if NETCOREAPP
        // For .NET Core, use the HttpContext to get the request-scoped service provider
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
            scopedProvider = httpContext.RequestServices;
#else
        // For .NET Framework (DNN), get the scope from HttpContext.Current.Items
        var httpContext = HttpContext.Current;
        if (httpContext != null)
        {
            var scope = httpContext.Items[typeof(IServiceScope)] as IServiceScope;
            scopedProvider = scope?.ServiceProvider;
        }
#endif

        // Fall back to the injected service provider if no scoped provider is available
        var provider = scopedProvider ?? serviceProvider;
        
        // Build a fresh converter instance from the request scope
        // This ensures it gets the current request's culture from IZoneCultureResolver
        return provider.Build<IConvertToEavLight>();
    }
}