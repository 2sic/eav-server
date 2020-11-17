using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class PersonsDataSource: DataSourceBase
    {
        private int _itemsToGenerate;
        private int _firstId;
        public override string LogId => "TST.Person";

        public PersonsDataSource()
        {
            Provide(GetPersons);
        }        

        public PersonsDataSource Init(int itemsToGenerate = 10, int firstId = 1001)
        {
            _itemsToGenerate = itemsToGenerate;
            _firstId = firstId;
            return this;
        }

        private IImmutableList<IEntity> GetPersons()
        {
            var persons = Person.GetSemiRandomList(_itemsToGenerate, _firstId);
            var list = Person.Person2Entity(persons);
            return list.ToImmutableArray();
        }
    }
}
