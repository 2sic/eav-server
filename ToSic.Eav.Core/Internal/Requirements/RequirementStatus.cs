using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements
{
    public class RequirementStatus
    {
        public RequirementStatus(bool isOk, Aspect aspect = default, string message = default)
        {
            IsOk = isOk;
            Aspect = aspect;
            Message = message;
        }

        public bool IsOk { get; }

        public Aspect Aspect { get; }

        public string Message { get; }
    }
}
