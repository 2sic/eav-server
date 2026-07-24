using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.DataSource;

namespace ToSic.Eav.TestData;

public class PersonsDataSource: DataSourceBase
{
    public PersonsDataSource(Dependencies services, DataAssembler dataAssembler, ContentTypeAssemblyKit ctAssemblyKit): base(services, "TST.Person")
    {
        _dataAssembler = dataAssembler;
        _ctAssemblyKit = ctAssemblyKit;
        ProvideOut(GetPersons);
    }

    private readonly DataAssembler _dataAssembler;
    private readonly ContentTypeAssemblyKit _ctAssemblyKit;

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
        var persons = new PersonGenerator(_dataAssembler, _ctAssemblyKit)
            .GetSemiRandomList(_itemsToGenerate, _firstId, new PersonSpecs());
        var list = new PersonGenerator(_dataAssembler, _ctAssemblyKit).Person2Entity(persons, _multiLanguage);
        return list.ToImmutableOpt();
    }
}