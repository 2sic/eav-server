namespace ToSic.Eav.DataSourceTests.TestData
{
    public class PersonSpecs
    {
        public const int DefaultItemsToGenerate = 10;
        public const int DefaultRootId = 1001;

        // ReSharper disable StringLiteralTypo

        public static string[] TestCities = { "Buchs", "Grabs", "Sevelen", "Zürich" };
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

        public const string FieldBirthday = "Birthdate";
        public const string FieldBirthdayNull = "BirthdateMaybeNull";

        public static string[] Fields = {
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
        };

        //public const string Col

    }
}
