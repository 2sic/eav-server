using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Web;
using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.WebApi.Sys.Helpers.Json;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavJsonConverter(IConvertToEavLight convertToEavLight) : JsonConverter<IEntity>
{
    public override IEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        var eavLightEntity = convertToEavLight.Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity.GetType(), options);
    }
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EavJsonConverterHttpEntry(IConvertToEavLight convertToEavLight) : JsonConverter<IEntity>
{
    public override IEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        var eavLightEntity = GetCurrentConverter().Convert(entity);
        JsonSerializer.Serialize(writer, eavLightEntity, eavLightEntity!.GetType(), options);
    }

    private IConvertToEavLight GetCurrentConverter()
    {
        return convertToEavLight;

        // @STV need your help here
#if !NETCOREAPP
        if (_currentConverter != null)
            return _currentConverter;
        // 1. Access the current HttpContext
        var httpContext = HttpContext.Current;
        if (httpContext == null)
            return _currentConverter = convertToEavLight;

        var scope = httpContext.Items[typeof(IServiceScope)] as IServiceScope;
        var sp = scope?.ServiceProvider;
        if (sp == null)
            return _currentConverter = convertToEavLight;
        return _currentConverter = sp.Build<IConvertToEavLight>();
#endif
    }

    private IConvertToEavLight? _currentConverter;
}