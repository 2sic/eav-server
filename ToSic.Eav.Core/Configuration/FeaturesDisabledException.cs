using System;
using System.Collections.Generic;

namespace ToSic.Eav.Configuration
{
    public class FeaturesDisabledException : Exception
    {
        //public FeaturesDisabledException(Guid feature): base(Features.MsgMissingSome(feature))
        //{ }

        public FeaturesDisabledException(string message, IEnumerable<Guid> features): base(message + " - " + Features.MsgMissingSome(features))
        { }
    }
}
