using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements
{
    public class RequirementError
    {
        public RequirementError(Requirement requirement, string message)
        {
            Requirement = requirement;
            Message = message;
        }

        public Requirement Requirement { get; set; }

        public string Message { get; set; }
    }
}
