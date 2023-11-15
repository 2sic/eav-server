using ToSic.Eav.Configuration;

namespace ToSic.Eav.Run.Requirements
{
    public class RequirementStatus
    {
        public RequirementStatus(bool isOk, AspectDefinition aspect = default, string message = default)
        {
            IsOk = isOk;
            Aspect = aspect;
            Message = message;
        }

        public bool IsOk { get; }

        public AspectDefinition Aspect { get; }

        public string Message { get; }
    }
}
