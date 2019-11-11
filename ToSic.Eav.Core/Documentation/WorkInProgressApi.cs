using System;

namespace ToSic.Eav.Documentation
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as public APIs
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class WorkInProgressApi: Attribute
    {
        public WorkInProgressApi(string message) { }
    }
}
