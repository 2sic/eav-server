namespace ToSic.Eav
{
    public class LogNames
    {
        /// <summary>
        /// Most common prefix in EAV systems
        /// </summary>
        public const string Eav = "Eav";
        
        /// <summary>
        /// Anything using this prefix allows the program to run, but doesn't do anything useful.
        /// Like a security check which always says not-allowed, or an exporter which doesn't export
        /// </summary>
        public const string NotImplemented = "NOT";

        /// <summary>
        /// Anything using this prefix is a basic implementation which works, but doesn't
        /// support anything more advanced / specific to a platform. It's usually fallback implementation.
        /// </summary>
        public const string Basic = "BAS";
    }
}
