using System;

namespace ToSic.Eav.PublicApi
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as public APIs
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class PublicApi: Attribute
    {
    }
}
