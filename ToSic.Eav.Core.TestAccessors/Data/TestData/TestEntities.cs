using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Data.TestData;

public static class TestEntities
{
    public const int AppId = -1;

    private static IContentType CtTestType(this DataBuilder builder) => builder.ContentType.CreateContentTypeTac(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
        {
            builder.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0),
            builder.ContentTypeAttributeTac(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
        }
    );


    private static IContentType CtPet(this DataBuilder builder) => builder.ContentType.CreateContentTypeTac(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
        {
            builder.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
            //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
            builder.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0)
        }
    );

    public const string AnyDateKey = "AnyDate";
    public const string AnyDateString = "2019-11-06T01:00:05Z";

    public static IEntity TestEntityDaniel(this DataBuilder builder)
    {
        var valDaniel = new Dictionary<string, object>
        {
            { "FirstName", "Daniel" },
            { "LastName", "Mettler" },
            { "Phone", "+41 81 750 67 70" },
            { "Age", 37 },
            { AnyDateKey, DateTime.Parse(AnyDateString) }
        };
        var entDaniel = builder.CreateEntityTac(appId: AppId, entityId: 1, contentType: builder.CtTestType(), values: valDaniel, titleField: "FirstName");
        return entDaniel;
    }

    public static IEntity TestEntityLeonie(this DataBuilder builder)
    {
        var valLeonie = new Dictionary<string, object>
        {
            { "FirstName", "Leonie" },
            { "LastName", "Mettler" },
            { "Phone", "+41 81 xxx yy zz" },
            { "Age", 6 }
        };

        var entLeonie = builder.CreateEntityTac(appId: AppId, entityId: 2, contentType: builder.CtTestType(), values: valLeonie, titleField: "FirstName");
        return entLeonie;
    }
    public static IEntity TestEntityPet(this DataBuilder builder, int petNumber)
    {
        var valsPet = new Dictionary<string, object>
        {
            { "FirstName", "PetNo" + petNumber },
            { "LastName", "Of Bonsaikitten" },
            { "Phone", "+41 81 xxx yy zz" },
            { "Age", petNumber }
        };

        var entPet = builder.CreateEntityTac(appId: AppId, entityId: 1000 + petNumber, contentType: builder.CtPet(), values: valsPet, titleField: "FirstName");
        return entPet;
    }
}