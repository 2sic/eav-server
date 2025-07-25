﻿using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;

namespace ToSic.Eav.TestData;

public class PersonsDataSource: DataSourceBase
{
    public PersonsDataSource(Dependencies services, DataBuilder dataBuilder): base(services, "TST.Person")
    {
        _dataBuilder = dataBuilder;
        ProvideOut(GetPersons);
    }

    private readonly DataBuilder _dataBuilder;

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
        var persons = new PersonGenerator(_dataBuilder).GetSemiRandomList(_itemsToGenerate, _firstId);
        var list = new PersonGenerator(_dataBuilder).Person2Entity(persons, _multiLanguage);
        return list.ToImmutableOpt();
    }
}