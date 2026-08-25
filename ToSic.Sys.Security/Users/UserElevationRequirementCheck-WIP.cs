//using ToSic.Sys.Capabilities;
//using ToSic.Sys.Capabilities.Aspects;
//using ToSic.Sys.Requirements;

//namespace ToSic.Sys.Users;

// TODO:
// TO IMPLEMENT user elevation, we must decide if
// a. we create one for each elevation
// b. we introduce parameters to the requirement check, so we can check for a specific elevation

//public class UserElevationRequirementCheck(IUser features) : RequirementCheckBase
//{
//    public override string NameId => FeatureConstants.RequirementUserElevationPrefix;

//    public override bool IsOk(Requirement requirement)
//        => features.GetElevation().IsAtLeast(UserElevation.All);

//    public override string InfoIfNotOk(Requirement requirement) 
//        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";

//    protected override Aspect GetAspect(Requirement requirement)
//        => features.Value.Get(requirement.NameId)?.Aspect ?? Aspect.None;

//}
