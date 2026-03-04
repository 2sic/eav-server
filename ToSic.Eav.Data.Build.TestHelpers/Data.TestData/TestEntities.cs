using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.TestData;

public static class TestEntities
{
    public const int AppId = -1;

    extension(ContentTypeAssembler dataAssembler)
    {
        private IContentType CtTestType() => dataAssembler.Type.CreateContentTypeTac(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
            {
                dataAssembler.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0),
                dataAssembler.ContentTypeAttributeTac(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
            }
        );

        private IContentType CtPet() => dataAssembler.Type.CreateContentTypeTac(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
            {
                dataAssembler.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
                //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
                dataAssembler.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0)
            }
        );
    }


    public const string AnyDateKey = "AnyDate";
    public const string AnyDateString = "2019-11-06T01:00:05Z";

    extension(DataAssembler dataAssembler)
    {
        public IEntity TestEntityDaniel(ContentTypeAssembler typeAssembler)
        {
            var valDaniel = new Dictionary<string, object>
            {
                { "FirstName", "Daniel" },
                { "LastName", "Mettler" },
                { "Phone", "+41 81 750 67 70" },
                { "Age", 37 },
                { AnyDateKey, DateTime.Parse(AnyDateString) }
            };
            var entDaniel = dataAssembler.CreateEntityTac(appId: AppId, entityId: 1, contentType: typeAssembler.CtTestType(), values: valDaniel, titleField: "FirstName");
            return entDaniel;
        }

        public IEntity TestEntityLeonie(ContentTypeAssembler typeAssembler)
        {
            var valLeonie = new Dictionary<string, object>
            {
                { "FirstName", "Leonie" },
                { "LastName", "Mettler" },
                { "Phone", "+41 81 xxx yy zz" },
                { "Age", 6 }
            };

            var entLeonie = dataAssembler.CreateEntityTac(appId: AppId, entityId: 2, contentType: typeAssembler.CtTestType(), values: valLeonie, titleField: "FirstName");
            return entLeonie;
        }

        public IEntity TestEntityPet(ContentTypeAssembler typeAssembler, int petNumber)
        {
            var valsPet = new Dictionary<string, object>
            {
                { "FirstName", "PetNo" + petNumber },
                { "LastName", "Of Bonsaikitten" },
                { "Phone", "+41 81 xxx yy zz" },
                { "Age", petNumber }
            };

            var entPet = dataAssembler.CreateEntityTac(appId: AppId, entityId: 1000 + petNumber, contentType: typeAssembler.CtPet(), values: valsPet, titleField: "FirstName");
            return entPet;
        }
    }
}