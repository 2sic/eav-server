// ReSharper disable once CheckNamespace

#region Enable Init Properties

namespace System.Runtime.CompilerServices
{

    // enable C# 9 init-only properties
    // https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
    internal static class IsExternalInit;
}

#endregion


#region required feature (for records / init)

namespace System.Runtime.CompilerServices
{
    public class RequiredMemberAttribute : Attribute;

    public class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name)
        { }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute;
}

#endregion