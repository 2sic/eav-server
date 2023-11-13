using ToSic.Eav.Configuration;

namespace ToSic.Eav.Run.Requirements
{
    public class RequirementStatus
    {
        public RequirementStatus(string nameId, bool isOk, AspectDefinition aspect = default, string message = default)
        {
            NameId = nameId;
            IsOk = isOk;
            Aspect = aspect;
            Message = message;
        }

        string NameId { get; }

        public bool IsOk { get; }

        public AspectDefinition Aspect { get; }

        public string Message { get; }
    }
}
