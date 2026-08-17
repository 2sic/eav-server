using ToSic.Sys.Utils.Assemblies;
using ToSic.Sys.DI;

namespace ToSic.Sys.Utils.Types;

public static class ServiceProviderExtensions
{
    public static Result<T> BuildByName<T>(this IServiceProvider sp, string fullName) where T : class
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return new(null, null, "Type name is null or empty");
        
        // generate an object of the specified type name
        var type = AssemblyHandling.GetTypeOrNull(fullName);

        if (type == null)
            return new(null, null, $"Type {fullName} not found");

        // Check if the type is ok, before instantiating it, to avoid security issues with instantiating random types.
        // It must be a data processor, otherwise it is not valid for this purpose.
        if (!typeof(T).IsAssignableFrom(type))
            return new(null, null, $"Type is not assignable from {nameof(T)}");

        // Re-verify it's a dataProcessor and not null
        var dataProcessor = sp.Build<T>(type);
        if (dataProcessor is not { })
            return new(null, null, $"Could not instantiate type {type}");

        return new(type, dataProcessor as T, $"Successfully created instance of {type}");
    }

    public record Result<T>(Type? Type, T? Instance, string Message);

}
