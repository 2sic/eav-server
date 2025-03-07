namespace ToSic.Eav.DataSourceTests.TestData;

public class PersonSpecs
{
    public const int DefaultItemsToGenerate = 10;
    public const int DefaultRootId = 1001;

    // Test Types
    public const bool UseDataTable = true;
    public const bool UseEntitiesL = false;

    // ReSharper disable StringLiteralTypo

    public const string City1 = "Buchs";
    public static string[] TestCities = [City1, "Grabs", "Sevelen", "Zürich"];
    // ReSharper restore StringLiteralTypo

    public static int MinHeight = 150;
    public static int HeightVar = 55;
    public static int IsMaleForEveryX = 3;

    public const string PersonTypeName = "Person";

    public const int ValueColumns = 10;
    public const string FieldFullName = "FullName";
    public const string FieldFirstName = "FirstName";
    public const string FieldLastName = "LastName";
    public const string FieldCity = "City";
    public const string FieldIsMale = "IsMale";
    public const string FieldHeight = "Height";
    public const string FieldCityMaybeNull = "CityMaybeNull";
    public const string FieldModifiedInternal = "InternalModified";
    public const string FieldBioForMlSortTest = "Biography";

    public const string BioMaleNoLangLast = "The man is...";
    public const string BioFemaleNoLangFirst = "The lady is...";
    public const string BioMaleDeFirst = "Er ist...";
    public const string BioFemaleDeLast = "Sie ist...";
    public const string BioMaleEnLast = "His personality is...";
    public const string BioFemaleEnFirst = "Her personality is...";
    public const string MlSortMalesFirst = "MF";
    public const string MlSortWomanFirst = "WF";
    public const string MlSortMixedOrder = "MO"; // this would be when the sort doesn't actually change the order


    public const string FieldBirthday = "Birthdate";
    public const string FieldBirthdayNull = "BirthdateMaybeNull";

    public static string[] Fields =
    [
        // the id won't be listed as a field
        //DataTable.EntityIdDefaultColumnName,
        FieldFullName, 
        FieldFirstName, 
        FieldLastName, 
        FieldCity, 
        FieldIsMale, 
        FieldHeight, 
        FieldCityMaybeNull, 
        FieldModifiedInternal
    ];

    // Languages we'll use in test-data
    // Important: we'll use a fake language as the primary
    // to better test all scenarios
    internal const string En = "en";
    internal const string EnUs = "en-US";
    internal const string De = "de";
    internal const string DeDe = "de-DE";
    internal const string DeCh = "de-CH";
    internal const string Fr = "fr";
    internal const string FrFr = "fr-FR";
    internal const string Pri = "pr";
    internal const string PrPr = "pr-PR";
    internal static Language LangPri = new Language(PrPr, false); // { DimensionId = 0, Key = PrPr};
    internal static Language LangEn = new Language(EnUs, false, 1); // { DimensionId = 1, Key = EnUs };
    internal static Language LangDeDe = new Language(DeDe, false, 42); // { DimensionId = 42, Key = DeDe };
    internal static Language LangDeCh = new Language (DeCh, false, 39); // { DimensionId = 39, Key = DeCh };
    internal static Language LangFr = new Language (FrFr, false, 99); // { DimensionId = 99, Key = FrFr };

    internal const string PriPrefix = "PR-";
    internal const string EnPrefix = "EN-";
    internal const string FrPrefix = "FR-";
    internal const string DeMult = "DE-Multi-";
}