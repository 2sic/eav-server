namespace ToSic.Eav.Configuration
{
    public class FeatureSecurity
    {
        public int Impact { get; }= 0;

        public string Message { get; }

        public FeatureSecurity(int impact, string message = "")
        {
            Impact = impact;
            Message = message;
        }
    }
}
