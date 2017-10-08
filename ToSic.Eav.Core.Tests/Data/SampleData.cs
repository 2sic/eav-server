using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Core.Tests.Data
{
    public static class SampleData
    {
        public const int AppId = -1;


        public static Interfaces.IEntity TestEntityDaniel()
        {
            var valDaniel = new Dictionary<string, object>()
            {
                {"FirstName", "Daniel"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 750 67 70"},
                {"Age", 37}
            };
            var entDaniel = new Entity(AppId, 1, "TestType", valDaniel, "FirstName");
            return entDaniel;
        }

        public static Interfaces.IEntity TestEntityLeonie()
        {
            var valLeonie = new Dictionary<string, object>()
            {
                {"FirstName", "Leonie"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", 6}
            };

            var entLeonie = new Entity(AppId, 2, "TestType", valLeonie, "FirstName");
            return entLeonie;
        }
        public static Interfaces.IEntity TestEntityPet(int petNumber)
        {
            var valsPet = new Dictionary<string, object>()
            {
                {"FirstName", "PetNo" + petNumber},
                {"LastName", "Of Bonsaikitten"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", petNumber}
            };

            var entPet = new Entity(AppId, 1000 + petNumber, "Pet", valsPet, "FirstName");
            return entPet;
        }
        

    }
}
