namespace ToSic.Eav.Configuration
{
    public class ConditionError
    {
        public ConditionError(Condition condition, string message)
        {
            Condition = condition;
            Message = message;
        }

        public Condition Condition { get; set; }

        public string Message { get; set; }
    }
}
