namespace ToSic.Eav.SysData
{
    public class FeatureSecurity
    {
        public int Impact { get; }

        public string Message { get; }

        public FeatureSecurity(int impact, string message = "")
        {
            Impact = impact;
            Message = message;
        }

        /// <summary>
        /// For fallback in null-cases, probably not used ATM
        /// </summary>
        public static FeatureSecurity Unknown = new(0, Constants.NullNameId);
    }
}
