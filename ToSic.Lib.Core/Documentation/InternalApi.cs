using System;

namespace ToSic.Lib.Documentation
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as public APIs
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    // ReSharper disable once InconsistentNaming
    public class InternalApi_DoNotUse_MayChangeWithoutNotice: Attribute
    {
        public InternalApi_DoNotUse_MayChangeWithoutNotice() { }

        public InternalApi_DoNotUse_MayChangeWithoutNotice(string comment = null) { }

    }
}
