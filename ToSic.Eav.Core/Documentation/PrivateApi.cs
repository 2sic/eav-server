using System;

namespace ToSic.Eav.Documentation
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as private APIs
    /// So they should not be publicly documented
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class PrivateApi: Attribute
    {
        public PrivateApi() { }
        public PrivateApi(string comment = null) { }
    }
}
