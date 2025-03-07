using ToSic.Eav.Apps;

namespace ToSic.Eav.RelationshipTests;

public class RelationshipTestSpecs
{
    public static IAppIdentity AppIdentity = new AppIdentity(2, 3);

    public const string Company = "Company";
    public const string Categories = "Categories";
    public const string Country = "Country";
    public const string Person = "Person";

    // IDs of special tests
    public const int PersonWithCompany = 726;
    public const int PersonCompanyCount = 1;

    public const int CompanyIdWithCountryAnd4Categories = 750;

    public const int CountrySwitzerland = 732;
    public const int CountrySwitzerlandParents = 2;

        
}