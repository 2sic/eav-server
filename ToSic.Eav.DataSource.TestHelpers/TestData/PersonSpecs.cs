using ToSic.Eav.Data;
using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.TestData;

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
    public const string En = "en";
    public const string EnUs = "en-US";
    public const string De = "de";
    public const string DeDe = "de-DE";
    public const string DeCh = "de-CH";
    public const string Fr = "fr";
    public const string FrFr = "fr-FR";
    public const string Pri = "pr";
    public const string PrPr = "pr-PR";
    public static Language LangPri = new(PrPr, false); // { DimensionId = 0, Key = PrPr};
    public static Language LangEn = new(EnUs, false, 1); // { DimensionId = 1, Key = EnUs };
    public static Language LangDeDe = new(DeDe, false, 42); // { DimensionId = 42, Key = DeDe };
    public static Language LangDeCh = new(DeCh, false, 39); // { DimensionId = 39, Key = DeCh };
    public static Language LangFr = new(FrFr, false, 99); // { DimensionId = 99, Key = FrFr };

    public const string PriPrefix = "PR-";
    public const string EnPrefix = "EN-";
    public const string FrPrefix = "FR-";
    public const string DeMult = "DE-Multi-";
}