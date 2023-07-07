using System;

namespace ToSic.Lib.Exceptions
{
    /// <summary>
    /// Exception to throw in scenarios where it may be shown to any user, and we want to be sure the stack trace is secret.
    /// </summary>
    public class PublicException: Exception
    {
        public PublicException(string message) : base(message) { }

        /// <summary>
        /// Don't display call stack as it's irrelevant
        /// </summary>
        public override string StackTrace => "";
    }
}
