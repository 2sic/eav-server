using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class PersonsDataSource: DataSourceBase
    {
        public override string LogId => "TST.Person";

        public PersonsDataSource()
        {
            Provide(GetPersons);
        }        

        public PersonsDataSource Init(int itemsToGenerate = 10, int firstId = 1001, bool multiLanguage = false)
        {
            _itemsToGenerate = itemsToGenerate;
            _firstId = firstId;
            _multiLanguage = multiLanguage;
            return this;
        }
        private int _itemsToGenerate;
        private int _firstId;
        private bool _multiLanguage;

        private IImmutableList<IEntity> GetPersons()
        {
            var persons = Person.GetSemiRandomList(_itemsToGenerate, _firstId);
            var list = Person.Person2Entity(persons, _multiLanguage);
            return list.ToImmutableArray();
        }
    }
}
