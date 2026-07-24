using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.TestData;

public static class TestEntities
{
    public const int AppId = -1;

    extension(ContentTypeAssemblyKit ctAssemblyKit)
    {
        private IContentType CtTestType() => ctAssemblyKit.Type.CreateContentTypeTac(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
            {
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
            }
        );

        private IContentType CtPet() => ctAssemblyKit.Type.CreateContentTypeTac(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
            {
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
                //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
                ctAssemblyKit.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0)
            }
        );
    }


    public const string AnyDateKey = "AnyDate";
    public const string AnyDateString = "2019-11-06T01:00:05Z";

    extension(DataAssembler dataAssembler)
    {
        public IEntity TestEntityDaniel(ContentTypeAssemblyKit ctAssemblyKit)
        {
            var valDaniel = new Dictionary<string, object>
            {
                { "FirstName", "Daniel" },
                { "LastName", "Mettler" },
                { "Phone", "+41 81 750 67 70" },
                { "Age", 37 },
                { AnyDateKey, DateTime.Parse(AnyDateString) }
            };
            var entDaniel = dataAssembler.CreateEntityTac(appId: AppId, entityId: 1, contentType: ctAssemblyKit.CtTestType(), values: valDaniel, titleField: "FirstName");
            return entDaniel;
        }

        public IEntity TestEntityLeonie(ContentTypeAssemblyKit ctAssemblyKit)
        {
            var valLeonie = new Dictionary<string, object>
            {
                { "FirstName", "Leonie" },
                { "LastName", "Mettler" },
                { "Phone", "+41 81 xxx yy zz" },
                { "Age", 6 }
            };

            var entLeonie = dataAssembler.CreateEntityTac(appId: AppId, entityId: 2, contentType: ctAssemblyKit.CtTestType(), values: valLeonie, titleField: "FirstName");
            return entLeonie;
        }

        public IEntity TestEntityPet(ContentTypeAssemblyKit ctAssemblyKit, int petNumber)
        {
            var valsPet = new Dictionary<string, object>
            {
                { "FirstName", "PetNo" + petNumber },
                { "LastName", "Of Bonsaikitten" },
                { "Phone", "+41 81 xxx yy zz" },
                { "Age", petNumber }
            };

            var entPet = dataAssembler.CreateEntityTac(appId: AppId, entityId: 1000 + petNumber, contentType: ctAssemblyKit.CtPet(), values: valsPet, titleField: "FirstName");
            return entPet;
        }
    }
}