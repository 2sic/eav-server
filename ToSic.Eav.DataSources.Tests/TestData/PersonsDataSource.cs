﻿using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests.TestData
{
    public class PersonsDataSource: DataSourceBase
    {
        public override string LogId => "TST.Person";

        public PersonsDataSource(AttributeBuilder attributeBuilder)
        {
            _attributeBuilder = attributeBuilder;
            Provide(GetPersons);
        }
        private AttributeBuilder _attributeBuilder;

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
            var persons = new PersonGenerator(_attributeBuilder).GetSemiRandomList(_itemsToGenerate, _firstId);
            var list = new PersonGenerator(_attributeBuilder).Person2Entity(persons, _multiLanguage);
            return list.ToImmutableArray();
        }
    }
}
