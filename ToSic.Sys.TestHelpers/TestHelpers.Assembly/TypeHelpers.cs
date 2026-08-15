namespace ToSic.Sys.TestHelpers.Assembly;

public static class TypeHelper
{
    public static Type GetTypeFromName(string className)
    {
        // 1. Get the current assembly where this test code is running
        var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();

        return currentAssembly.GetTypeFromName(className);
    }
    
    
    public static Type GetTypeFromName(this System.Reflection.Assembly assembly, string className)
    {
        // 2. Search for the type by its simple name
        var targetType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Equals(className, StringComparison.Ordinal));

        // 3. Handle the case where the class wasn't found
        return targetType
               ?? throw new ArgumentException($"Type '{className}' not found in the current assembly.");
    }
}
