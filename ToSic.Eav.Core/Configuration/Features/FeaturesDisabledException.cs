using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Typed exception so code can check if the exception was a feature-exception
    /// </summary>
    [PrivateApi]
    public class FeaturesDisabledException : Exception
    {
        public FeaturesDisabledException(string message, IEnumerable<Guid> features): base(message)
        { }
    }
}
